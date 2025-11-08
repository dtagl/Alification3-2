// File: Program.cs
using Api.Data;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.HttpOverrides;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Log startup information
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"PORT: {Environment.GetEnvironmentVariable("PORT") ?? "not set"}");
Console.WriteLine($"DATABASE_URL: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")) ? "not set" : "set")}");
Console.WriteLine($"REDIS_URL: {(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDIS_URL")) ? "not set" : "set")}");

// Add services (global auth by default, but exclude Swagger and health endpoints)
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Api", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    // Make security requirement optional (not required for Swagger UI access)
    options.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddScoped<TelegramAuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IHomepageService, HomepageService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// HTTP Client Factory для Telegram Bot API
builder.Services.AddHttpClient();

// Telegram Notification Service
builder.Services.AddScoped<ITelegramNotificationService, TelegramNotificationService>();

// Background Service for booking notifications
builder.Services.AddHostedService<BookingNotificationBackgroundService>();

// DB: Railway provides DATABASE_URL, fallback to ConnectionStrings:Default
var dbConnectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(dbConnectionString))
{
    // Try Railway's DATABASE_URL format: postgresql://user:pass@host:port/db
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            // Parse Railway DATABASE_URL format
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            if (userInfo.Length >= 2)
            {
                dbConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={Uri.UnescapeDataString(userInfo[1])};SSL Mode=Require;Trust Server Certificate=true";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse DATABASE_URL: {ex.Message}");
        }
    }
}

if (string.IsNullOrEmpty(dbConnectionString))
{
    Console.WriteLine("Warning: Database connection string is not configured. Application may not work correctly.");
    // Don't throw - allow app to start for healthcheck
}

if (!string.IsNullOrEmpty(dbConnectionString))
{
    builder.Services.AddDbContext<MyContext>(options =>
        options.UseNpgsql(dbConnectionString)
    );
}
else
{
    // Add a minimal context that won't crash on startup
    builder.Services.AddDbContext<MyContext>(options =>
        options.UseNpgsql("Host=localhost;Port=5432;Database=temp;Username=temp;Password=temp")
    );
}

// Redis: Railway provides REDIS_URL, fallback to ConnectionStrings:Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? builder.Configuration.GetValue<string>("Redis:Configuration")
    ?? Environment.GetEnvironmentVariable("REDIS_URL");

if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "rooms:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// CORS (allow Telegram webapp origin during dev; adjust in prod)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// JWT Authentication (services)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? string.Empty;
if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("Warning: JWT Key is not configured. Authentication will not work.");
    // Use a default key for development (should be set in production)
    jwtKey = "default-key-change-in-production-" + Guid.NewGuid().ToString();
}

var key = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(jwtSection["Issuer"]),
            ValidateAudience = !string.IsNullOrEmpty(jwtSection["Audience"]),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtKey),
            ValidIssuer = jwtSection["Issuer"] ?? "Default",
            ValidAudience = jwtSection["Audience"] ?? "Default",
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Убираем запас времени для проверки срока действия
        };
        
        // Правильная обработка заголовка Authorization
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException)
                {
                    context.Response.Headers["Token-Expired"] = "true";
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Trust forwarded headers (for Railway proxy or nginx reverse proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Railway uses a proxy, so we need to trust it
    KnownNetworks = { },
    KnownProxies = { }
});

// Apply database migrations on startup (non-blocking)
_ = Task.Run(async () =>
{
    await Task.Delay(2000); // Wait a bit for DB to be ready
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Attempting to connect to database...");
        if (await context.Database.CanConnectAsync())
        {
            logger.LogInformation("Database connection successful. Applying migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogWarning("Cannot connect to database. Migrations will be skipped.");
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database. Application will continue.");
    }
});

app.UseCors("AllowLocal");
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in all environments (useful for Railway healthcheck and API documentation)
// Swagger must be after UseAuthorization to work properly
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Room Booking API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();

// Log that application is starting
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
logger.LogInformation("Application starting on port {Port}", port);
logger.LogInformation("Health endpoint available at: /api/health");
logger.LogInformation("Swagger UI available at: /swagger");

app.Run();
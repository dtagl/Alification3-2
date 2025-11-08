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

var builder = WebApplication.CreateBuilder(args);

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
        // Parse Railway DATABASE_URL format
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        dbConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}

builder.Services.AddDbContext<MyContext>(options =>
    options.UseNpgsql(dbConnectionString ?? throw new InvalidOperationException("Database connection string is required"))
);

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
var key = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
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

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

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

app.Run();
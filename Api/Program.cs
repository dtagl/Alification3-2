// File: Program.cs
using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<TelegramAuthService>();

// DB: replace "Default" connection string in appsettings.json
builder.Services.AddDbContext<MyContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

// CORS (allow Telegram webapp origin during dev; adjust in prod)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocal");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
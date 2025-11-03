// File: Controllers/FirstController.cs
using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/first")]
public class FirstController : ControllerBase
{
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;

    public FirstController(TelegramAuthService auth, MyContext context)
    {
        _auth = auth;
        _context = context;
    }

    // Create company + admin user
    [HttpPost("create-company")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.CompanyName) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Company name and password required.");

        var exists = await _context.Companies.AnyAsync(c => c.Name == dto.CompanyName);
        if (exists) return Conflict("Company already exists.");

        var company = new Company
        {
            Name = dto.CompanyName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        // Create initial admin user if telegram id provided
        if (dto.TelegramId.HasValue)
        {
            var user = new User
            {
                TelegramId = dto.TelegramId.Value,
                UserName = dto.UserName ?? "admin",
                CompanyId = company.Id,
                Role = Role.Admin
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return Ok(new { company.Id });
    }

    // Login or register by Telegram id (basic)
    [HttpPost("login-telegram")]
    public async Task<IActionResult> LoginTelegram([FromBody] TelegramLoginDto dto)
    {
        if (dto == null || dto.TelegramId == 0) return BadRequest();

        // NOTE: ideally validate widget with TelegramAuthService (dto.Data)
        // For now assume telegram data valid if provided
        var user = await _context.Users.Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.TelegramId == dto.TelegramId);

        if (user != null) return Ok(new { user.Id, user.UserName, user.Role });

        // Register as basic User — company must be passed
        if (dto.CompanyId == Guid.Empty) return BadRequest("CompanyId required for new users.");

        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null) return NotFound("Company not found.");

        var newUser = new User
        {
            TelegramId = dto.TelegramId,
            UserName = dto.UserName ?? $"tg_{dto.TelegramId}",
            CompanyId = company.Id,
            Role = Role.User
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { newUser.Id, newUser.UserName });
    }
}

// DTOs
public record CreateCompanyDto(string CompanyName, string Password, long? TelegramId, string UserName);
public record TelegramLoginDto(long TelegramId, Guid CompanyId, string UserName, Dictionary<string, string> Data);

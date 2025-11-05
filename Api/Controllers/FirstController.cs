// File: Controllers/FirstController.cs
using Api.Data;
using Api.Data.Entities;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/first")]
[AllowAnonymous]
public class FirstController : ControllerBase
{
    private readonly TelegramAuthService _auth;
    private readonly MyContext _context;
    private readonly IConfiguration _config;

    public FirstController(TelegramAuthService auth, MyContext context, IConfiguration config)
    {
        _auth = auth;
        _context = context;
        _config = config;
    }

    private string GenerateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.TryParse(jwt["ExpiryMinutes"], out var m) ? m : 120);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("companyId", user.CompanyId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Create company + admin user (returns JWT token + companyId)
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            WorkingStart = dto.WorkingStart,
            WorkingEnd = dto.WorkingEnd
        };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        // Create initial admin user (required for token generation)
        User adminUser;
        if (dto.TelegramId.HasValue)
        {
            // Check if user already exists with this TelegramId
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == dto.TelegramId.Value);
            if (existingUser != null)
            {
                // Update existing user to be admin of this company
                existingUser.CompanyId = company.Id;
                existingUser.Role = Role.Admin;
                if (!string.IsNullOrWhiteSpace(dto.UserName))
                    existingUser.UserName = dto.UserName;
                await _context.SaveChangesAsync();
                adminUser = existingUser;
            }
            else 
            {
                adminUser = new User
                {
                    TelegramId = dto.TelegramId.Value,
                    UserName = string.IsNullOrWhiteSpace(dto.UserName) ? "admin" : dto.UserName,
                    CompanyId = company.Id,
                    Role = Role.Admin
                };
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // Create admin user without TelegramId
            adminUser = new User
            {
                TelegramId = 0,
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? "admin" : dto.UserName,
                CompanyId = company.Id,
                Role = Role.Admin
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        var token = GenerateToken(adminUser);
        return Ok(new { token});
    }

    // Login or register by Telegram id (returns JWT)
    [HttpPost("login-telegram")]
    public async Task<IActionResult> LoginTelegram([FromBody] TelegramLoginDto dto)
    {
        if (dto == null || dto.TelegramId == 0) return BadRequest();

        // Try existing user first
        var user = await _context.Users.Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.TelegramId == dto.TelegramId);

        if (user == null)
        {
            if (dto.CompanyId == Guid.Empty) return BadRequest("CompanyId required for new users.");
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null) return NotFound("Company not found.");

            user = new User
            {
                TelegramId = dto.TelegramId,
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? $"tg_{dto.TelegramId}" : dto.UserName,
                CompanyId = company.Id,
                Role = Role.User
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateToken(user);
        return Ok(new { token});
    }
    
    //this is firstlayer entrypage to test if user is already logined (returns JWT token + companyId)
    [HttpPost("entrypage")]
    public async Task<IActionResult> EntryPage([FromBody] EntryPageDto dto)
    {
        if (dto.telegramId == 0) return BadRequest();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == dto.telegramId);
        if (user == null) return NotFound("User not found.");
        
        var token = GenerateToken(user);
        return Ok(new { token});
    }
}

// DTOs
public record EntryPageDto(long telegramId);
public record TelegramLoginDto(long TelegramId, Guid CompanyId, string? UserName);

public record CreateCompanyDto(string CompanyName, string Password, long? TelegramId, string UserName, TimeSpan WorkingStart,TimeSpan WorkingEnd);

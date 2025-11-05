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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            WorkingStart = dto.WorkingStart,
            WorkingEnd = dto.WorkingEnd
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

    //this should be changed cuz logic is wrong
    //
    //should be: user enter endpoint and user only gives tgid, and if this telegram id is in db, then it should return companyid
    //else if there is no such id in db then it should open register or login page.
    //
    // Login or register by Telegram id (basic)
    [HttpPost("login-telegram")]
    public async Task<IActionResult> LoginTelegram([FromBody] TelegramLoginDto dto)
    {
        if (dto == null || dto.TelegramId == 0) return BadRequest();

        // NOTE: ideally validate widget with TelegramAuthService (dto.Data)
        
        // Register as basic User — company must be passed
        if (dto.CompanyId == Guid.Empty) return BadRequest("CompanyId required for new users.");

        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null) return NotFound("Company not found.");

        var newUser = new User
        {
            TelegramId = dto.TelegramId,
            UserName = dto.UserName,
            CompanyId = company.Id,
            Role = Role.User
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { newUser.Id, newUser.CompanyId, newUser.Role });
    }
    //my version
    //and after this if there is no such user it should go to the first page
    //if there is such id then we are returning company id to frontend to open company's homepage and skip this login menu
    [HttpPost("entrypage")]
    public async Task<IActionResult> EntryPage([FromBody] EntryPageDto dto)
    {
        if (dto.telegramId == 0) return BadRequest();
        var user = _context.Users.FirstOrDefault(u => u.TelegramId == dto.telegramId);
        if (user == null) return NotFound("User not found.");
        else return Ok(new{user.Id, user.CompanyId, user.Role} );
    }
}

// DTOs
public record EntryPageDto(long telegramId);
public record CreateCompanyDto(string CompanyName, string Password, long? TelegramId, string UserName, TimeSpan WorkingStart,TimeSpan WorkingEnd);
public record TelegramLoginDto(long TelegramId, Guid CompanyId, string UserName, Dictionary<string, string> Data);

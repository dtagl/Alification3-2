// File: Controllers/HomepageController.cs
using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/home")]
public class HomepageController : ControllerBase
{
    private readonly MyContext _context;
    public HomepageController(MyContext context) => _context = context;

    // My bookings split by active / expired
    [HttpGet("my-bookings/{telegramId:long}")]
    public async Task<IActionResult> MyBookings(long telegramId)
    {
        var now = DateTime.UtcNow;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramId == telegramId);

        if (user == null)
            return NotFound();

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == user.Id)
            .ToListAsync();

        var active = bookings.Where(b => b.EndAt >= now).ToList();
        var past = bookings.Where(b => b.EndAt < now).ToList();

        return Ok(new { active, past });
    }


    // Available rooms now (basic: rooms with no ongoing booking at now)
    [HttpGet("available-now/{companyId:guid}")]
    public async Task<IActionResult> AvailableNow(Guid companyId)
    {
        var now = DateTime.UtcNow;
        var rooms = await _context.Rooms
            .Include(r => r.Bookings)
            .Where(r => r.CompanyId == companyId)
            .ToListAsync();

        var avail = rooms.Where(r => !r.Bookings.Any(b => b.StartAt <= now && b.EndAt > now))
            .Select(r => new { r.Id, r.Name, r.Capacity, r.Description });
        return Ok(avail);
    }
}
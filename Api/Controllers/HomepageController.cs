// File: Controllers/HomepageController.cs
using Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/home")]
[Authorize]
public class HomepageController : ControllerBase
{
    private readonly MyContext _context;
    public HomepageController(MyContext context) => _context = context;

    // My bookings split by active / expired
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var now = DateTime.UtcNow;
        var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? HttpContext.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Forbid();

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        var active = bookings.Where(b => b.EndAt >= now).ToList();
        var past = bookings.Where(b => b.EndAt < now).ToList();

        return Ok(new { active, past });
    }


    // Available rooms now (basic: rooms with no ongoing booking at now)
    [HttpGet("available-now")]
    public async Task<IActionResult> AvailableNow()
    {
        var now = DateTime.UtcNow;
        var companyIdClaim = HttpContext.User.FindFirst("companyId")?.Value;
        if (!Guid.TryParse(companyIdClaim, out var companyId)) return Forbid();
        var rooms = await _context.Rooms
            .Include(r => r.Bookings)
            .Where(r => r.CompanyId == companyId)
            .ToListAsync();

        var avail = rooms.Where(r => !r.Bookings.Any(b => b.StartAt <= now && b.EndAt > now))
            .Select(r => new { r.Id, r.Name, r.Capacity, r.Description });
        return Ok(avail);
    }
}
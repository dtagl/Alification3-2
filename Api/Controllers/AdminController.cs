// File: Controllers/AdminController.cs
using Api.Data;
using Api.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly MyContext _context;
    public AdminController(MyContext context) => _context = context;

    private async Task<bool> IsAdmin(Guid requesterId)
    {
        var user = await _context.Users.FindAsync(requesterId);
        return user is { Role: Role.Admin };
    }

    // ---------------------------
    // ✅ 1. Company overview
    // ---------------------------
    [HttpGet("overview/{companyId:guid}")]
    public async Task<IActionResult> GetCompanyOverview(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var totalRooms = await _context.Rooms.CountAsync(r => r.CompanyId == companyId);
        var totalUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId);
        var totalBookings = await _context.Bookings
            .Include(b => b.Room)
            .CountAsync(b => b.Room.CompanyId == companyId);

        var activeBookings = await _context.Bookings
            .Include(b => b.Room)
            .CountAsync(b => b.Room.CompanyId == companyId && b.EndAt > DateTime.UtcNow);

        return Ok(new
        {
            totalRooms,
            totalUsers,
            totalBookings,
            activeBookings
        });
    }

    // ---------------------------
    // ✅ 2. Room utilization (percentage of booked time)
    // ---------------------------
    [HttpGet("room-utilization/{companyId:guid}")]
    public async Task<IActionResult> GetRoomUtilization(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var rooms = await _context.Rooms
            .Where(r => r.CompanyId == companyId)
            .Include(r => r.Bookings)
            .ToListAsync();

        var stats = rooms.Select(r =>
        {
            var totalHours = 8 * 5; // workweek baseline 8h x 5 days
            var bookedHours = r.Bookings
                .Where(b => b.StartAt >= DateTime.UtcNow.AddDays(-7))
                .Sum(b => (b.EndAt - b.StartAt).TotalHours);

            return new
            {
                Room = r.Name,
                UtilizationPercent = Math.Round(bookedHours / totalHours * 100, 2)
            };
        });

        return Ok(stats);
    }

    // ---------------------------
    // ✅ 3. Top 5 most used rooms
    // ---------------------------
    [HttpGet("top-rooms/{companyId:guid}")]
    public async Task<IActionResult> GetTopRooms(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var topRooms = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Room.CompanyId == companyId)
            .GroupBy(b => b.Room.Name)
            .Select(g => new { Room = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return Ok(topRooms);
    }

    // ---------------------------
    // ✅ 4. User activity stats
    // ---------------------------
    [HttpGet("user-activity/{companyId:guid}")]
    public async Task<IActionResult> GetUserActivity(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var activity = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Where(b => b.Room.CompanyId == companyId)
            .GroupBy(b => b.User.UserName)
            .Select(g => new
            {
                User = g.Key,
                Bookings = g.Count(),
                TotalHours = g.Sum(x => (x.EndAt - x.StartAt).TotalHours)
            })
            .OrderByDescending(x => x.Bookings)
            .ToListAsync();

        return Ok(activity);
    }

    // ---------------------------
    // ✅ 5. Daily booking trends (last 7 days)
    // ---------------------------
    [HttpGet("bookings-trend/{companyId:guid}")]
    public async Task<IActionResult> GetBookingTrends(Guid companyId, [FromQuery] Guid requesterId)
    {
        if (!await IsAdmin(requesterId)) return Forbid();

        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var trend = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Room.CompanyId == companyId && b.StartAt >= weekAgo)
            .GroupBy(b => b.StartAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(trend);
    }
}

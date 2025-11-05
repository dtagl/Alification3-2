// File: Controllers/RoomController.cs
using Api.Data;
using Api.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly MyContext _context;
    public RoomController(MyContext context) => _context = context;

    // List all rooms for a company
    [HttpGet("company/{companyId:guid}")]
    public async Task<IActionResult> GetCompanyRooms(Guid companyId)
    {
        var rooms = await _context.Rooms.Where(r => r.CompanyId == companyId)
                                       .Select(r => new { r.Id, r.Name, r.Capacity, r.Description })
                                       .ToListAsync();
        return Ok(rooms);
    }

    
    // Admin: create room
    [HttpPost("create")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
    {
        if (dto == null) return BadRequest();
        var company = await _context.Companies.FindAsync(dto.CompanyId);
        if (company == null) return NotFound("Company not found.");

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Description = dto.Description,
            CompanyId = dto.CompanyId
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return Ok(new { room.Id });
    }
    
    
    [HttpGet("{roomId:guid}/timeslots")]
    public async Task<IActionResult> GetAvailableTimeslots(Guid roomId, [FromQuery] DateTime date)
    {
        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

        var room = await _context.Rooms.Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return NotFound();

        var company = room.Company;
        var start = date.Date.Add(company.WorkingStart);
        var end = date.Date.Add(company.WorkingEnd);

        // get all bookings for this room on this date
        var bookings = await _context.Bookings
            .Where(b => b.RoomId == roomId && b.StartAt.Date == date.Date)
            .ToListAsync();

        var slots = new Dictionary<DateTime, bool>();

        for (var time = start; time < end; time = time.AddMinutes(15))
        {
            // find if any booking overlaps with this slot
            bool isBooked = bookings.Any(b =>
                b.StartAt <= time && b.EndAt > time);

            // booked → false (not available)
            slots[time] = !isBooked;
        }

        return Ok(slots);
    }



    // Book a room (basic overlap check)
    [HttpPost("{roomId:guid}/book")]
    public async Task<IActionResult> BookRoom(Guid roomId, [FromBody] BookDto dto)
    {
        var room = await _context.Rooms.Include(r => r.Bookings).FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return NotFound("Room not found.");

        // Validate times
        if (dto.StartAt >= dto.EndAt) return BadRequest("Invalid time range.");
        // Optional: ensure within working hours e.g., 9:00-18:00 (server local)
        // Check overlapping bookings
        var overlap = room.Bookings.Any(b => !(dto.EndAt <= b.StartAt || dto.StartAt >= b.EndAt));
        if (overlap) return Conflict("Time slot occupied.");

        var booking = new Booking
        {
            RoomId = roomId,
            UserId = dto.UserId,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            TimespanId = ComputeTimespanId(dto.StartAt) // optional
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return Ok(new { booking.Id });
    }

    // Cancel own booking (or admin can cancel any if extended)
    [HttpDelete("booking/{bookingId:guid}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId, [FromQuery] Guid userId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null) return NotFound();

        // allow cancel if requester is owner or admin (simple check)
        var requester = await _context.Users.FindAsync(userId);
        if (requester == null) return Forbid();

        if (requester.Role != Role.Admin && booking.UserId != userId)
            return Forbid();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return Ok();
    }

    private int ComputeTimespanId(DateTime t)
    {
        // timespan index within day (0..95); naive UTC->local not handled
        return (int)(t.TimeOfDay.TotalMinutes / 15);
    }
}

// DTOs
public record CreateRoomDto(Guid CompanyId, string Name, int Capacity, string Description);
public record BookDto(Guid UserId, DateTime StartAt, DateTime EndAt);

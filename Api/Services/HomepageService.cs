using Api.Contracts.Homepage;
using Api.Contracts.Rooms;
using Api.Data;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class HomepageService : IHomepageService
{
    private readonly MyContext _context;

    public HomepageService(MyContext context)
    {
        _context = context;
    }

    public async Task<MyBookingsDto> GetMyBookingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Project to DTOs to avoid circular reference issues
        // No need for Include() when using Select() - EF Core will automatically include what's needed
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Select(b => new MyBookingDto(
                b.Id,
                b.RoomId,
                b.Room.Name,
                b.StartAt,
                b.EndAt
            ))
            .ToListAsync(cancellationToken);

        var active = bookings.Where(b => b.EndAt >= now).ToList();
        var past = bookings.Where(b => b.EndAt < now).ToList();

        return new MyBookingsDto(active, past);
    }

    public async Task<IEnumerable<RoomSummaryDto>> GetAvailableNowAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var rooms = await _context.Rooms
            .Include(r => r.Bookings)
            .Where(r => r.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        var available = rooms
            .Where(r => !r.Bookings.Any(b => b.StartAt <= now && b.EndAt > now))
            .Select(r => new RoomSummaryDto(r.Id, r.Name, r.Capacity, r.Description))
            .ToList();

        return available;
    }
}


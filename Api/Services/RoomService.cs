using System.Text.Json;
using Api.Contracts.Rooms;
using Api.Data;
using Api.Data.Entities;
using Api.Services.Exceptions;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Api.Services;

public class RoomService : IRoomService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly MyContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RoomService> _logger;

    public RoomService(MyContext context, IDistributedCache cache, ILogger<RoomService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<RoomSummaryDto>> GetCompanyRoomsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var rooms = await _context.Rooms
            .Where(r => r.CompanyId == companyId)
            .Select(r => new RoomSummaryDto(r.Id, r.Name, r.Capacity, r.Description))
            .ToListAsync(cancellationToken);

        return rooms;
    }

    public async Task<Guid> CreateRoomAsync(Guid companyId, CreateRoomDto dto, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies.FindAsync([companyId], cancellationToken);
        if (company == null)
            throw new KeyNotFoundException("Company not found.");

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Description = dto.Description,
            CompanyId = companyId
        };

        await _context.Rooms.AddAsync(room, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return room.Id;
    }

public async Task<IDictionary<DateTime, bool>> GetAvailableTimeslotsAsync(
    Guid roomId,
    Guid companyId,
    DateTime date,
    CancellationToken cancellationToken = default)
{
    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
    var cacheKey = BuildTimeslotCacheKey(roomId, date);

    try
    {
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached != null)
        {
            var cachedEntries = JsonSerializer.Deserialize<List<TimeslotCacheEntry>>(cached, SerializerOptions);
            if (cachedEntries != null)
                return cachedEntries.ToDictionary(e => e.Start, e => e.IsAvailable);
        }
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to read room timeslots from cache for {RoomId} on {Date}", roomId, date.Date);
    }

    // Проверяем, что комната принадлежит компании
    var room = await _context.Rooms
        .Include(r => r.Company)
        .FirstOrDefaultAsync(r => r.Id == roomId && r.CompanyId == companyId, cancellationToken);

    if (room == null)
        throw new KeyNotFoundException("Room not found.");

    var company = room.Company;
    var start = date.Date.Add(company.WorkingStart);
    var end = date.Date.Add(company.WorkingEnd);

    // Загружаем бронирования, пересекающиеся с рабочим днём
    var bookings = await _context.Bookings
        .Where(b => b.RoomId == roomId && b.StartAt < end && b.EndAt > start)
        .ToListAsync(cancellationToken);

    var slots = new Dictionary<DateTime, bool>();

    for (var time = start; time < end; time = time.AddMinutes(15))
    {
        var slotStart = time;
        var slotEnd = time.AddMinutes(15);

        // Проверяем пересечение интервалов
        var isBooked = bookings.Any(b =>
            b.StartAt < slotEnd && b.EndAt > slotStart.AddSeconds(1));

        slots[slotStart] = !isBooked;
    }

    try
    {
        var cachePayload = JsonSerializer.Serialize(
            slots.Select(kvp => new TimeslotCacheEntry(kvp.Key, kvp.Value)),
            SerializerOptions);

        await _cache.SetStringAsync(cacheKey, cachePayload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to cache room timeslots for {RoomId} on {Date}", roomId, date.Date);
    }

    return slots;
}

    public async Task<Guid> BookRoomAsync(Guid roomId, Guid userId, BookRoomDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.StartAt >= dto.EndAt)
            throw new ArgumentException("Invalid time range.");

        var room = await _context.Rooms.Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
        if (room == null)
            throw new KeyNotFoundException("Room not found.");

        var overlap = room.Bookings.Any(b => !(dto.EndAt <= b.StartAt || dto.StartAt >= b.EndAt));
        if (overlap)
            throw new ConflictException("Time slot occupied.");

        var booking = new Booking
        {
            RoomId = roomId,
            UserId = userId,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            TimespanId = ComputeTimespanId(dto.StartAt)
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        await InvalidateTimeslotsCacheAsync(roomId, dto.StartAt, dto.EndAt, cancellationToken);

        return booking.Id;
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync([bookingId], cancellationToken);
        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        var requester = await _context.Users.FindAsync([requesterId], cancellationToken);
        if (requester == null)
            throw new ForbiddenException("Requester not found.");

        // Allow: Admin cancels any; or user cancels own booking
        if (requester.Role != Role.Admin && booking.UserId != requesterId)
            throw new ForbiddenException("Not allowed to cancel this booking.");

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync(cancellationToken);

        await InvalidateTimeslotsCacheAsync(booking.RoomId, booking.StartAt, booking.EndAt, cancellationToken);
    }

    public async Task<IEnumerable<RoomSearchResultDto>> FindRoomsAsync(Guid companyId, RoomFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter.StartAt.HasValue && filter.EndAt.HasValue && filter.StartAt.Value >= filter.EndAt.Value)
            throw new ArgumentException("Start time must be before end time.");

        var query = _context.Rooms
            .Include(r => r.Bookings)
            .Where(r => r.CompanyId == companyId)
            .AsQueryable();

        if (filter.MinCapacity.HasValue)
            query = query.Where(r => r.Capacity >= filter.MinCapacity.Value);

        if (filter.MaxCapacity.HasValue)
            query = query.Where(r => r.Capacity <= filter.MaxCapacity.Value);

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(r => EF.Functions.ILike(r.Name, $"%{filter.Name}%"));

        if (!string.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(r => EF.Functions.ILike(r.Description, $"%{filter.Description}%"));

        if (filter.StartAt.HasValue && filter.EndAt.HasValue)
        {
            var start = filter.StartAt.Value;
            var end = filter.EndAt.Value;
            query = query.Where(r => !r.Bookings.Any(b => b.StartAt < end && b.EndAt > start));
        }

        var rooms = await query.ToListAsync(cancellationToken);

        var results = rooms.Select(r =>
        {
            var isAvailable = true;
            if (filter.StartAt.HasValue && filter.EndAt.HasValue)
            {
                var start = filter.StartAt.Value;
                var end = filter.EndAt.Value;
                isAvailable = !r.Bookings.Any(b => b.StartAt < end && b.EndAt > start);
            }

            return new RoomSearchResultDto(r.Id, r.Name, r.Capacity, r.Description, isAvailable);
        });

        return results;
    }

    public async Task<BookingInfoDto?> GetBookingInfoAsync(Guid roomId, Guid companyId, DateTime time, CancellationToken cancellationToken = default)
    {
        time = DateTime.SpecifyKind(time, DateTimeKind.Utc);

        // Validate room exists and belongs to the company (optimized query with companyId filter)
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.CompanyId == companyId, cancellationToken);
        if (room == null)
        {
            throw new KeyNotFoundException("Room not found.");
        }

        // Находим бронирование, которое включает это время
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Where(b => b.RoomId == roomId && b.StartAt <= time && b.EndAt > time)
            .FirstOrDefaultAsync(cancellationToken);

        if (booking == null)
        {
            // Время свободно
            return new BookingInfoDto(null, time, time, false);
        }

        // Время забронировано
        return new BookingInfoDto(booking.User.UserName, booking.StartAt, booking.EndAt, true);
    }

    private static string BuildTimeslotCacheKey(Guid roomId, DateTime date) => $"room:{roomId}:timeslots:{date:yyyyMMdd}";

    private async Task InvalidateTimeslotsCacheAsync(Guid roomId, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        var dates = new HashSet<DateTime> { start.Date, end.Date };
        foreach (var date in dates)
        {
            var cacheKey = BuildTimeslotCacheKey(roomId, date);
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for {RoomId} on {Date}", roomId, date);
            }
        }
    }

    private static int ComputeTimespanId(DateTime t) => (int)(t.TimeOfDay.TotalMinutes / 15);

    private record TimeslotCacheEntry(DateTime Start, bool IsAvailable);
}


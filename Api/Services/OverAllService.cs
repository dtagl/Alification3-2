using Api.Contracts.Homepage;
using Api.Data;
using Api.Data.Entities;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class OverAllService:IOverAllService
{
    private readonly MyContext _context;
    public OverAllService(MyContext context)
    {
        _context=context;
    }
    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users.ToListAsync(cancellationToken);
        return users;
    }
    public async Task<IEnumerable<Room>> GetAllRoomsAsync(CancellationToken cancellationToken = default)
    {
        var rooms = await _context.Rooms.ToListAsync(cancellationToken);
        return rooms;
    }
    public async Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings.ToListAsync(cancellationToken);
        return bookings;
    }
    public async Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _context.Companies.ToListAsync(cancellationToken);
        return companies;
    }
}
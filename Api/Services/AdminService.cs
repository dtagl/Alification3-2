using Api.Contracts.Admin;
using Api.Services.Interfaces;
using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AdminService : IAdminService
{
    private readonly MyContext _context;

    public AdminService(MyContext context)
    {
        _context = context;
    }

    //cancelation token added but not used in controller yet
    public async Task<CompanyOverviewDto> GetCompanyOverviewAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var totalRooms = await _context.Rooms.CountAsync(r => r.CompanyId == companyId, cancellationToken);
        var totalUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId, cancellationToken);
        // No need for Include() - EF Core will automatically join when accessing navigation properties
        var totalBookings = await _context.Bookings
            .CountAsync(b => b.Room.CompanyId == companyId, cancellationToken);

        var activeBookings = await _context.Bookings
            .CountAsync(b => b.Room.CompanyId == companyId && b.EndAt > DateTime.UtcNow, cancellationToken);

        return new CompanyOverviewDto(totalRooms, totalUsers, totalBookings, activeBookings);
    }

    public async Task<IEnumerable<RoomUtilizationDto>> GetRoomUtilizationAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var rooms = await _context.Rooms
            .Where(r => r.CompanyId == companyId)
            .Include(r => r.Bookings)
            .ToListAsync(cancellationToken);

        var stats = rooms.Select(r =>
        {
            var totalHours = 8 * 5;
            var bookedHours = r.Bookings
                .Where(b => b.StartAt >= DateTime.UtcNow.AddDays(-7))
                .Sum(b => (b.EndAt - b.StartAt).TotalHours);

            var utilization = totalHours == 0 ? 0 : Math.Round(bookedHours / totalHours * 100, 2);
            return new RoomUtilizationDto(r.Name, utilization);
        });

        return stats;
    }

    public async Task<IEnumerable<TopRoomDto>> GetTopRoomsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        // Use a subquery approach that EF Core can translate properly
        var topRooms = await _context.Rooms
            .Where(r => r.CompanyId == companyId)
            .Select(r => new
            {
                RoomName = r.Name,
                BookingCount = _context.Bookings.Count(b => b.RoomId == r.Id)
            })
            .OrderByDescending(x => x.BookingCount)
            .Take(5)
            .Select(x => new TopRoomDto(x.RoomName, x.BookingCount))
            .ToListAsync(cancellationToken);

        return topRooms;
    }

    public async Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        // Fetch bookings first, then group in memory to avoid EF Core translation issues
        var bookings = await _context.Bookings
            .Where(b => b.Room.CompanyId == companyId)
            .Select(b => new
            {
                UserName = b.User.UserName,
                StartAt = b.StartAt,
                EndAt = b.EndAt
            })
            .ToListAsync(cancellationToken);

        // Group and calculate in memory
        var activity = bookings
            .GroupBy(b => b.UserName)
            .Select(g => new UserActivityDto(
                g.Key,
                g.Count(),
                g.Sum(x => (x.EndAt - x.StartAt).TotalHours)
            ))
            .OrderByDescending(x => x.Bookings)
            .ToList();

        return activity;
    }

    public async Task<IEnumerable<BookingTrendDto>> GetBookingTrendsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        // Fetch bookings first, then group in memory to avoid EF Core translation issues with Date property
        var bookings = await _context.Bookings
            .Where(b => b.Room.CompanyId == companyId && b.StartAt >= weekAgo)
            .Select(b => new { b.StartAt })
            .ToListAsync(cancellationToken);

        // Group and count in memory
        var trend = bookings
            .GroupBy(b => b.StartAt.Date)
            .Select(g => new BookingTrendDto(g.Key, g.Count()))
            .OrderBy(x => x.Date)
            .ToList();

        return trend;
    }

    public async Task<IEnumerable<AdminUserSummaryDto>> GetAllUsersAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Where(u => u.CompanyId == companyId)
            .Select(u => new AdminUserSummaryDto(u.Id, u.UserName, u.Role))
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<UserRoleChangeDto> MakeAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Role = Role.Admin;
        await _context.SaveChangesAsync(cancellationToken);
        return new UserRoleChangeDto(user.Id, user.Role);
    }

    public async Task<UserRoleChangeDto> RevokeAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.Role = Role.User;
        await _context.SaveChangesAsync(cancellationToken);
        return new UserRoleChangeDto(user.Id, user.Role);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CompanyWorkingHoursDto> ChangeCompanyWorkingHoursAsync(Guid companyId, ChangeWorkingHoursDto dto, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies.FindAsync([companyId], cancellationToken);
        if (company == null)
            throw new KeyNotFoundException("Company not found.");

        company.WorkingStart = dto.WorkingStart;
        company.WorkingEnd = dto.WorkingEnd;
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyWorkingHoursDto(company.Id, company.WorkingStart, company.WorkingEnd);
    }

    public async Task<CompanyNameDto> ChangeCompanyNameAsync(Guid companyId, string newName, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies.FindAsync([companyId], cancellationToken);
        if (company == null)
            throw new KeyNotFoundException("Company not found.");

        company.Name = newName;
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyNameDto(company.Id, company.Name);
    }

    public async Task<CompanyPasswordChangeDto> ChangeCompanyPasswordAsync(Guid companyId, string newPassword, CancellationToken cancellationToken = default)
    {
        var company = await _context.Companies.FindAsync([companyId], cancellationToken);
        if (company == null)
            throw new KeyNotFoundException("Company not found.");

        company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return new CompanyPasswordChangeDto(company.Id, "Password changed successfully.");
    }
}


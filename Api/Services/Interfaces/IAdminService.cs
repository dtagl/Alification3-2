using Api.Contracts.Admin;

namespace Api.Services.Interfaces;

public interface IAdminService
{
    Task<CompanyOverviewDto> GetCompanyOverviewAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomUtilizationDto>> GetRoomUtilizationAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopRoomDto>> GetTopRoomsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingTrendDto>> GetBookingTrendsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdminUserSummaryDto>> GetAllUsersAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<UserRoleChangeDto> MakeAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserRoleChangeDto> RevokeAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CompanyWorkingHoursDto> ChangeCompanyWorkingHoursAsync(Guid companyId, ChangeWorkingHoursDto dto, CancellationToken cancellationToken = default);
    Task<CompanyNameDto> ChangeCompanyNameAsync(Guid companyId, string newName, CancellationToken cancellationToken = default);
    Task<CompanyPasswordChangeDto> ChangeCompanyPasswordAsync(Guid companyId, string newPassword, CancellationToken cancellationToken = default);
}


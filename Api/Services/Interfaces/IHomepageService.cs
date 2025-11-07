using Api.Contracts.Homepage;
using Api.Contracts.Rooms;

namespace Api.Services.Interfaces;

public interface IHomepageService
{
    Task<MyBookingsDto> GetMyBookingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomSummaryDto>> GetAvailableNowAsync(Guid companyId, CancellationToken cancellationToken = default);
}


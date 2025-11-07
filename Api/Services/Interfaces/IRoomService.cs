using Api.Contracts.Rooms;

namespace Api.Services.Interfaces;

public interface IRoomService
{
    Task<IEnumerable<RoomSummaryDto>> GetCompanyRoomsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Guid> CreateRoomAsync(Guid companyId, CreateRoomDto dto, CancellationToken cancellationToken = default);
    Task<IDictionary<DateTime, bool>> GetAvailableTimeslotsAsync(Guid roomId, Guid companyId, DateTime date, CancellationToken cancellationToken = default);
    Task<Guid> BookRoomAsync(Guid roomId, Guid userId, BookRoomDto dto, CancellationToken cancellationToken = default);
    Task CancelBookingAsync(Guid bookingId, Guid requesterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoomSearchResultDto>> FindRoomsAsync(Guid companyId, RoomFilterDto filter, CancellationToken cancellationToken = default);
    Task<BookingInfoDto?> GetBookingInfoAsync(Guid roomId, Guid companyId, DateTime time, CancellationToken cancellationToken = default);
}


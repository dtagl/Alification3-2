using Api.Data.Entities;

namespace Api.Services.Interfaces;

public interface IOverAllService
{
    public Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Room>> GetAllRoomsAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);
    public Task<IEnumerable<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);

}
namespace Api.Contracts.Rooms;

public record RoomSearchResultDto(Guid Id, string Name, int Capacity, string Description, bool IsAvailable);


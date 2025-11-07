namespace Api.Contracts.Rooms;

public record RoomSummaryDto(Guid Id, string Name, int Capacity, string Description);


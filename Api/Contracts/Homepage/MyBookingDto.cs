namespace Api.Contracts.Homepage;

public record MyBookingDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    DateTime StartAt,
    DateTime EndAt
);


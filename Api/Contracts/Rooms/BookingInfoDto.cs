namespace Api.Contracts.Rooms;

public record BookingInfoDto(string? UserName, DateTime StartAt, DateTime EndAt, bool IsBooked);


namespace Api.Contracts.Homepage;

public record MyBookingsDto(List<MyBookingDto> Active, List<MyBookingDto> Past);


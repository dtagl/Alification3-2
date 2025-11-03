// File: Data/Entities/Booking.cs
namespace Api.Data.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid RoomId { get; set; }
    public Room Room { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    // timespan index 0..95 for 15-min slots of a day (optional)
    public int TimespanId { get; set; }
}
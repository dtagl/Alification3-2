// File: Data/Entities/Room.cs
using System.ComponentModel.DataAnnotations;

namespace Api.Data.Entities;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = default!;

    public int Capacity { get; set; } = 1;

    public string Description { get; set; } = default!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
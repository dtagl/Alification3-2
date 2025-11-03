// File: Data/Entities/User.cs
using System.ComponentModel.DataAnnotations;

namespace Api.Data.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Telegram provided id
    public long TelegramId { get; set; }

    public string UserName { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; }

    public Role Role { get; set; } = Role.User;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
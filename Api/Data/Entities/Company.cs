// File: Data/Entities/Company.cs
using System.ComponentModel.DataAnnotations;

namespace Api.Data.Entities;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; }

    // hashed password for admin access to company management (basic)
    [Required]
    public string PasswordHash { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
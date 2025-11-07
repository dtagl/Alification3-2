namespace Api.Data.Entities;

public class NotificationSent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // Тип уведомления: "10min_before" для уведомления за 10 минут
    public string NotificationType { get; set; } = "10min_before";
}


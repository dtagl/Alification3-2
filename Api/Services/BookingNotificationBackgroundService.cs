using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Services;

public class BookingNotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingNotificationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Проверка каждую минуту
    private readonly TimeSpan _notificationTimeBefore = TimeSpan.FromMinutes(10); // Уведомление за 10 минут

    public BookingNotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingNotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingNotificationBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking bookings for notifications");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("BookingNotificationBackgroundService stopped");
    }

    private async Task CheckAndSendNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<ITelegramNotificationService>();

        var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tashkent"); // UZT
        var nowUtc = DateTime.UtcNow;

// Конвертируем текущее UTC-время в локальное время пользователя
        var nowUser = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, userTimeZone);

// Время уведомления через 10 минут
        var targetTimeUser = nowUser + _notificationTimeBefore;

// Делаем ±1 минуту для поиска бронирований
        var targetTimeStartUser = targetTimeUser.AddMinutes(-1);
        var targetTimeEndUser = targetTimeUser.AddMinutes(1);

// Конвертируем обратно в UTC для сравнения с базой
        var targetTimeStartUtc = TimeZoneInfo.ConvertTimeToUtc(targetTimeStartUser, userTimeZone);
        var targetTimeEndUtc = TimeZoneInfo.ConvertTimeToUtc(targetTimeEndUser, userTimeZone);

// Получаем бронирования
        var upcomingBookings = await context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .Where(b => b.StartAt >= targetTimeStartUtc && b.StartAt <= targetTimeEndUtc)
            .ToListAsync(cancellationToken);

        if (!upcomingBookings.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} upcoming bookings to check for notifications", upcomingBookings.Count);

        foreach (var booking in upcomingBookings)
        {
            try
            {
                // Проверяем, было ли уже отправлено уведомление для этого бронирования
                var alreadySent = await context.NotificationsSent
                    .AnyAsync(n => n.BookingId == booking.Id && n.NotificationType == "10min_before", cancellationToken);

                if (alreadySent)
                {
                    _logger.LogDebug("Notification already sent for booking {BookingId}", booking.Id);
                    continue;
                }

                // Проверяем, что у пользователя есть TelegramId
                if (booking.User.TelegramId <= 0)
                {
                    _logger.LogWarning("User {UserId} has no valid TelegramId for booking {BookingId}", booking.UserId, booking.Id);
                    continue;
                }

                // Отправляем уведомление
                await notificationService.SendBookingReminderAsync(booking.User.TelegramId, booking, cancellationToken);

                // Сохраняем запись об отправленном уведомлении
                var notificationSent = new NotificationSent
                {
                    BookingId = booking.Id,
                    NotificationType = "10min_before",
                    SentAt = now
                };

                context.NotificationsSent.Add(notificationSent);
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully sent notification for booking {BookingId} to user {TelegramId}", 
                    booking.Id, booking.User.TelegramId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification for booking {BookingId}", booking.Id);
                // Продолжаем обработку других бронирований даже если одно не удалось
            }
        }
    }
}


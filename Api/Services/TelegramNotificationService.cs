using Api.Data;
using Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface ITelegramNotificationService
{
    Task SendBookingReminderAsync(long telegramId, Booking booking, CancellationToken cancellationToken = default);
}

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly MyContext _context;
    private readonly ILogger<TelegramNotificationService> _logger;
    private readonly string _botToken;

    public TelegramNotificationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        MyContext context,
        ILogger<TelegramNotificationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _context = context;
        _logger = logger;
        _botToken = config["Telegram:BotToken"] ?? string.Empty;
    }

    public async Task SendBookingReminderAsync(long telegramId, Booking booking, CancellationToken cancellationToken = default)
    {
        if (telegramId <= 0)
        {
            _logger.LogWarning("Invalid TelegramId {TelegramId} for booking {BookingId}", telegramId, booking.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogWarning("BotToken is not configured. Cannot send notification for booking {BookingId}", booking.Id);
            return;
        }

        try
        {
            var room = await _context.Rooms.FindAsync([booking.RoomId], cancellationToken);
            if (room == null)
            {
                _logger.LogWarning("Room {RoomId} not found for booking {BookingId}", booking.RoomId, booking.Id);
                return;
            }

            var timeUntilBooking = booking.StartAt - DateTime.UtcNow.AddHours(5); // UZT
            var minutesUntilBooking = (int)timeUntilBooking.TotalMinutes;

            var message = $"📅 Напоминание о бронировании\n\n" +
                         $"Ваша бронь через {minutesUntilBooking} минут!\n\n" +
                         $"🏢 Комната: {room.Name}\n" +
                         $"📝 Описание: {room.Description}\n" +
                         $"⏰ Начало: {booking.StartAt:dd.MM.yyyy HH:mm}\n" +
                         $"⏰ Конец: {booking.EndAt:dd.MM.yyyy HH:mm}";

            // Отправляем сообщение через Telegram Bot API напрямую
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            
            // Telegram Bot API ожидает form-data
            var formData = new List<KeyValuePair<string, string>>
            {
                new("chat_id", telegramId.ToString()),
                new("text", message)
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Sent booking reminder to TelegramId {TelegramId} for booking {BookingId}", telegramId, booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking reminder to TelegramId {TelegramId} for booking {BookingId}", telegramId, booking.Id);
            throw;
        }
    }
}


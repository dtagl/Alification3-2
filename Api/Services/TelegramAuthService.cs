// File: Services/TelegramAuthService.cs
using System.Security.Cryptography;
using System.Text;

namespace Api.Services;

public class TelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration config)
    {
        _botToken = config["Telegram:BotToken"] ?? string.Empty;
    }

    // Validate Telegram login widget data dictionary (key -> value)
    // Returns true if signature valid. Accepts pre-parsed dictionary (without "hash")
    public bool Validate(Dictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(_botToken)) return false;
        if (!data.TryGetValue("hash", out var hash)) return false;

        // Build data_check_string: sort keys (except hash), join "k=v\n"
        var kv = data.Where(kv2 => kv2.Key != "hash")
            .OrderBy(kv2 => kv2.Key)
            .Select(kv2 => $"{kv2.Key}={kv2.Value}");
        var dataCheck = string.Join("\n", kv);

        // secret_key = sha256(bot_token)
        var secretKey = SHA256Hash(_botToken);
        // compute HMAC-SHA256(secret_key, data_check_string)
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheck));
        var hex = BitConverter.ToString(computed).Replace("-", "").ToLower();

        return hex == hash;
    }

    private static string SHA256Hash(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
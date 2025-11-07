


// не используется в текущей версии, но может пригодиться для будущей интеграции с Telegram
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Api.Services;

public class TelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration config)
    {
        _botToken = config["Telegram:BotToken"] ?? string.Empty;
    }

    // Парсит init_data (query string) и проверяет подпись
    public bool ValidateInitData(string initData)
    {
        if (string.IsNullOrEmpty(initData)) return false;
        // QueryHelpers.ParseQuery ожидает строку с ?, поэтому добавляем при необходимости
        var qs = initData.StartsWith("?") ? initData : "?" + initData;
        var parsed = QueryHelpers.ParseQuery(qs);
        var dict = parsed.ToDictionary(k => k.Key, v => v.Value.ToString());
        return Validate(dict);
    }

    // Validate Telegram login widget / web app data (dictionary with "hash" present)
    public bool Validate(Dictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(_botToken)) return false;
        if (!data.TryGetValue("hash", out var hash)) return false;

        // Build data_check_string: sort keys (except hash), join "k=v\n"
        var kv = data.Where(kv2 => kv2.Key != "hash")
            .OrderBy(kv2 => kv2.Key)
            .Select(kv2 => $"{kv2.Key}={kv2.Value}");
        var dataCheck = string.Join("\n", kv);

        // secret_key = sha256(bot_token) (raw bytes)
        byte[] secretKeyBytes;
        using (var sha = SHA256.Create())
        {
            secretKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(_botToken));
        }

        using var hmac = new HMACSHA256(secretKeyBytes);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheck));
        var hex = BitConverter.ToString(computed).Replace("-", "").ToLowerInvariant();

        return hex == hash;
    }
}
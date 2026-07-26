using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.TelegramValidator
{
    public class TelegramValidator: ITelegramValidator
    {
        private readonly IConfiguration _configuration;

        public TelegramValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TelegramUser Validate(string initData)
        {
            if (string.IsNullOrWhiteSpace(initData))
                throw new UnauthorizedAccessException("Empty initData.");

            var botToken = _configuration["BotSettings:TELEGRAM_BOT_TOKEN"];

            if (string.IsNullOrWhiteSpace(botToken))
                throw new Exception("Telegram bot token is missing.");

            var values = ParseQueryString(initData);

            if (!values.TryGetValue("hash", out var receivedHash))
                throw new UnauthorizedAccessException("Hash missing.");

            values.Remove("hash");

            var dataCheckString = string.Join('\n',
                values
                    .OrderBy(x => x.Key)
                    .Select(x => $"{x.Key}={x.Value}")
            );

            // secret = HMAC_SHA256("WebAppData", botToken)
            using var hmacSecret = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
            var secret = hmacSecret.ComputeHash(Encoding.UTF8.GetBytes(botToken));

            using var hmac = new HMACSHA256(secret);

            var calculatedHash = Convert.ToHexString(
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(dataCheckString)))
                .ToLowerInvariant();

            if (!string.Equals(calculatedHash, receivedHash, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid Telegram signature.");

            // optional: reject old requests (>24h)
            if (values.TryGetValue("auth_date", out var authDateString))
            {
                var authDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(authDateString));

                if (DateTimeOffset.UtcNow - authDate > TimeSpan.FromHours(24))
                    throw new UnauthorizedAccessException("Expired Telegram login.");
            }

            if (!values.TryGetValue("user", out var userJson))
                throw new UnauthorizedAccessException("User missing.");

            var user = JsonSerializer.Deserialize<TelegramUser>(userJson);

            if (user == null)
                throw new UnauthorizedAccessException("Cannot parse Telegram user.");

            if (user.IsBot)
                throw new UnauthorizedAccessException("Bots are not allowed.");

            return user;
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();

            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = pair.IndexOf('=');

                if (idx <= 0)
                    continue;

                var key = Uri.UnescapeDataString(pair[..idx]);
                var value = Uri.UnescapeDataString(pair[(idx + 1)..]);

                result[key] = value;
            }

            return result;
        }
    }
}
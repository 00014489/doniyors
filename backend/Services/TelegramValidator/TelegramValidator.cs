using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using backend.Options;
using Microsoft.Extensions.Options;

namespace backend.Services.TelegramValidator
{
    public class TelegramValidator : ITelegramValidator
    {
        /// <summary>Telegram rejects logins older than this; so do we.</summary>
        private static readonly TimeSpan MaxAge = TimeSpan.FromHours(24);

        private static readonly byte[] SecretSalt = Encoding.UTF8.GetBytes("WebAppData");

        private readonly byte[] _secretKey;

        public TelegramValidator(IOptions<TelegramOptions> options)
        {
            // secret_key = HMAC_SHA256(key: "WebAppData", data: bot_token)
            // The token never changes at runtime, so derive the key once.
            _secretKey = HMACSHA256.HashData(
                SecretSalt,
                Encoding.UTF8.GetBytes(options.Value.BotToken));
        }

        public TelegramUser Validate(string initData)
        {
            if (string.IsNullOrWhiteSpace(initData))
                throw new UnauthorizedAccessException("Empty initData.");

            var values = ParseQueryString(initData);

            if (!values.Remove("hash", out var receivedHash))
                throw new UnauthorizedAccessException("Hash missing.");

            var dataCheckString = string.Join(
                '\n',
                values
                    .OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => $"{x.Key}={x.Value}"));

            var expected = HMACSHA256.HashData(
                _secretKey,
                Encoding.UTF8.GetBytes(dataCheckString));

            if (!IsHashEqual(expected, receivedHash))
                throw new UnauthorizedAccessException("Invalid Telegram signature.");

            EnsureFresh(values);

            if (!values.TryGetValue("user", out var userJson))
                throw new UnauthorizedAccessException("User missing.");

            TelegramUser? user;

            try
            {
                user = JsonSerializer.Deserialize<TelegramUser>(userJson);
            }
            catch (JsonException)
            {
                throw new UnauthorizedAccessException("Cannot parse Telegram user.");
            }

            if (user is null)
                throw new UnauthorizedAccessException("Cannot parse Telegram user.");

            if (user.IsBot)
                throw new UnauthorizedAccessException("Bots are not allowed.");

            return user;
        }

        /// <summary>
        /// Compared in constant time. A plain string comparison leaks, through
        /// how long it takes to fail, how much of a guessed hash was correct.
        /// </summary>
        private static bool IsHashEqual(byte[] expected, string receivedHex)
        {
            byte[] received;

            try
            {
                received = Convert.FromHexString(receivedHex);
            }
            catch (FormatException)
            {
                return false;
            }

            // Returns false on a length mismatch without short-circuiting.
            return CryptographicOperations.FixedTimeEquals(expected, received);
        }

        private static void EnsureFresh(Dictionary<string, string> values)
        {
            if (!values.TryGetValue("auth_date", out var authDateString))
                throw new UnauthorizedAccessException("auth_date missing.");

            if (!long.TryParse(authDateString, out var unixSeconds))
                throw new UnauthorizedAccessException("auth_date is malformed.");

            var authDate = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

            if (DateTimeOffset.UtcNow - authDate > MaxAge)
                throw new UnauthorizedAccessException("Expired Telegram login.");
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

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

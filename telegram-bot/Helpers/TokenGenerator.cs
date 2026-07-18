using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace telegram_bot.Helpers
{
    public static class TokenGenerator
    {
        public static string Generate()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }
    }
}
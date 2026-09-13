using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Doniyors.Data
{
    public static class TokenGenerator
    {
        public static string Generate()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }
    }
}
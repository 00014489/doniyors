using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class UserTransactionDto
    {
        public long TgUserId { get; set; }

        public string TravelTitle { get; set; } = string.Empty;

        public int Points { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string Season { get; set; } = string.Empty;
    }
}
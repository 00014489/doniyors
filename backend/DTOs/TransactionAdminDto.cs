using System;

namespace backend.DTOs
{
    public class TransactionAdminDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public long TgUserId { get; set; }

        /// <summary>Null when the transaction is a manual points adjustment.</summary>
        public int? TravelId { get; set; }
        public string TravelTitle { get; set; } = string.Empty;

        /// <summary>Signed points moved on the user's balance.</summary>
        public int Points { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>False when the transaction has been voided (soft deleted).</summary>
        public bool Status { get; set; }
    }
}

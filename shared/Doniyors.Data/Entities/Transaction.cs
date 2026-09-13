using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doniyors.Data.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Optional — a transaction may be a manual points adjustment that is
        /// not tied to any adventure.
        /// </summary>
        public int? TravelId { get; set; }

        /// <summary>
        /// Signed points this transaction moves on the user's balance:
        /// positive adds, negative subtracts. Only counted while <see cref="Status"/> is true.
        /// </summary>
        public int Points { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Soft-delete flag. <c>false</c> means the transaction is voided but
        /// the record is kept for auditing.
        /// </summary>
        public bool Status { get; set; } = true;

        public User User { get; set; } = null!;

        public Travel? Travel { get; set; }
    }
}
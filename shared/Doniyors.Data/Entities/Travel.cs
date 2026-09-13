using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Doniyors.Data.Entities
{
    public class Travel
    {
        public int Id { get; set; }
    
        public string Title { get; set; } = null!;

        /// <summary>
        /// What the adventure involves, shown on its detail page. Optional —
        /// adventures created before this existed simply have none.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        public DateTimeOffset TravelDate { get; set; }

        public decimal Cost { get; set; }

        public int Points { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Soft-delete flag. <c>false</c> means the adventure is disabled and
        /// must not be offered to end users, but the row is kept for history.
        /// </summary>
        public bool Status { get; set; } = true;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<TravelImage> Images { get; set; } = new List<TravelImage>();
    }
}
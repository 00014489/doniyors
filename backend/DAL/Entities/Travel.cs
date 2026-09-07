using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace backend.DAL.Entities
{
    public class Travel
    {
        public int Id { get; set; }
    
        public string Title { get; set; } = null!;

        public DateTimeOffset TravelDate { get; set; }

        public decimal Cost { get; set; }

        public int Points { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<TravelImage> Images { get; set; } = new List<TravelImage>();
    }
}
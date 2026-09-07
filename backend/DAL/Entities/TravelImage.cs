using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DAL.Entities
{
    public class TravelImage
    {
        public int Id { get; set; }

        public int TravelId { get; set; }

        public string Title { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Travel Travel { get; set; } = null!;
    }
}
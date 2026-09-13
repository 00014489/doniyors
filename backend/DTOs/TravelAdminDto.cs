using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class TravelAdminDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset TravelDate { get; set; }
        public decimal Cost { get; set; }
        public int Points { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>False when the adventure has been disabled (soft deleted).</summary>
        public bool Status { get; set; }

        /// <summary>Ordered images; the first one is the cover shown in the list.</summary>
        public List<TravelImageDto> Images { get; set; } = new();
    }
}

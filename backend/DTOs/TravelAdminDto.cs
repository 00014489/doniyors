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
        public DateTimeOffset TravelDate { get; set; }
        public decimal Cost { get; set; }
        public int Points { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doniyors.Data.Entities
{
    public class TravelImage
    {
        public int Id { get; set; }

        public int TravelId { get; set; }

        public string Title { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        /// <summary>Raw file bytes. Kept in the database so no external storage is required.</summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();

        /// <summary>Upload order. The lowest value is the adventure's cover image.</summary>
        public int SortOrder { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public Travel Travel { get; set; } = null!;
    }
}
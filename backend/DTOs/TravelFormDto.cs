using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.DTOs
{
    /// <summary>
    /// Multipart payload for creating or updating an adventure. Images travel with
    /// the form, so this is bound with <c>[FromForm]</c> rather than as JSON.
    /// </summary>
    public class TravelFormDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset TravelDate { get; set; }

        [Range(0, 99_999_999.99)]
        public decimal Cost { get; set; }

        [Range(0, int.MaxValue)]
        public int Points { get; set; }

        public bool Status { get; set; } = true;

        /// <summary>Newly uploaded files.</summary>
        public List<IFormFile> Images { get; set; } = new();

        /// <summary>
        /// On update: ids of the already stored images to keep. Anything not
        /// listed is removed.
        /// </summary>
        public List<int> KeepImageIds { get; set; } = new();

        public TravelSaveDto ToSaveDto() =>
            new TravelSaveDto
            {
                Title = Title,
                Description = Description,
                TravelDate = TravelDate,
                Cost = Cost,
                Points = Points,
                Status = Status
            };
    }
}

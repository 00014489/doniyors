using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// Payload used by the admin panel to create or update an adventure.
    /// The same shape is used for both operations so the client can reuse a single form.
    /// </summary>
    public class TravelSaveDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        /// <summary>Optional; shown on the adventure's detail page.</summary>
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset TravelDate { get; set; }

        [Range(0, 99_999_999.99)]
        public decimal Cost { get; set; }

        [Range(0, int.MaxValue)]
        public int Points { get; set; }

        /// <summary>Active flag. Defaults to active for newly created adventures.</summary>
        public bool Status { get; set; } = true;
    }
}

namespace backend.DTOs.Member
{
    /// <summary>One card in the mobile adventure list.</summary>
    public class AdventureListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset TravelDate { get; set; }

        public decimal Cost { get; set; }

        /// <summary>Points awarded for completing the adventure.</summary>
        public int Points { get; set; }

        /// <summary>
        /// The adventure's first image by sort order. Null only if an
        /// administrator managed to save one without pictures.
        /// </summary>
        public int? CoverImageId { get; set; }

        /// <summary>True while the travel date is still in the future.</summary>
        public bool IsUpcoming { get; set; }
    }

    /// <summary>The adventure detail page: everything, plus every picture.</summary>
    public class AdventureDetailDto : AdventureListItemDto
    {
        /// <summary>What the adventure involves. Empty when none was written.</summary>
        public string Description { get; set; } = string.Empty;

        public IReadOnlyList<TravelImageDto> Images { get; set; } = [];
    }
}

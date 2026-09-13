using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>Payload used by the admin panel to create or update a transaction.</summary>
    public class TransactionSaveDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        /// <summary>Optional — leave null for a manual points adjustment.</summary>
        [Range(1, int.MaxValue)]
        public int? TravelId { get; set; }

        /// <summary>Signed points: positive adds to the balance, negative subtracts.</summary>
        public int Points { get; set; }

        public bool Status { get; set; } = true;
    }
}

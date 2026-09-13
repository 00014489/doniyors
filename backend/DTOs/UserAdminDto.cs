using System;

namespace backend.DTOs
{
    public class UserAdminDto
    {
        public int Id { get; set; }
        public long TgUserId { get; set; }
        public string UserName { get; set; } = null!;
        public string LanguageCode { get; set; } = string.Empty;
        public int Points { get; set; }
        public int TypeUserId { get; set; }
        public string TypeUserName { get; set; } = string.Empty;
        public DateTimeOffset RegisteredAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>False when the account has been disabled (soft deleted).</summary>
        public bool Status { get; set; }
    }
}

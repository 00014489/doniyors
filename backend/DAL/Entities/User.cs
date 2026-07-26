using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }

        public long TgUserId { get; set; }

        public string UserName { get; set; } = null!;

        public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public int Points { get; set; } = 0;

        public string QrToken { get; set; } = null!;
        public string LanguageCode { get; set; } = string.Empty;

        public int TypeUserId { get; set; } = 3;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public TypeUser TypeUser { get; set; } = null!;
        public UserSession? Session { get; set; }
    }
}
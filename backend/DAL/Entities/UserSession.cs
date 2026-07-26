using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DAL.Entities
{
    public class UserSession
    {
        public long UserId { get; set; }           // PK + FK to User
        public SessionStep Step { get; set; } = SessionStep.None;
        public string DataJson { get; set; } = "{}"; // mapped to jsonb
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; } = null!;
    }
}
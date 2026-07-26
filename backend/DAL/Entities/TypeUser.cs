using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DAL.Entities
{
    public class TypeUser
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
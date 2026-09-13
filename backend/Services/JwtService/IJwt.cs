using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doniyors.Data.Entities;

namespace backend.Services.JwtService
{
    public interface IJwt
    {
        string Create(User user);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.AuthService
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string initData);
    }
}
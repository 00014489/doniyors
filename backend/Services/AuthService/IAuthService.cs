using backend.DTOs;

namespace backend.Services.AuthService
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(
            string initData,
            CancellationToken cancellationToken = default);
    }
}

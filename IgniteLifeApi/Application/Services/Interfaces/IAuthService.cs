using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Dtos.Common;
using IgniteLifeApi.Application.Services.Common;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ServiceResult<Unit>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<ServiceResult<Unit>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}

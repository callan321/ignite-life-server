using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Dtos.Common;
using IgniteLifeApi.Application.Services.Common;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResult<Unit>> LogoutAsync(string refreshToken);
        Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId);
        Task<ServiceResult<Unit>> RefreshTokensAsync(string refreshToken);
    }
}

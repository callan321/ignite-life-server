using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Dtos.Common;
using IgniteLifeApi.Application.Services.Common;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResult<Unit>> LogoutAsync(string refreshToken);
        Task<ServiceResult<Unit>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ServiceResult<Unit>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId);
    }
}

using System.Security.Claims;
using IgniteLifeApi.Application.Dtos.Auth;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(Guid userId, bool isPersistent);
        ClaimsPrincipal? ValidateAccessToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<TokenResponse?> RefreshTokensAsync(string refreshToken);
    }
}

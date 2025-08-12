using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Domain.Interfaces;
using System.Security.Claims;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(IJwtUser user, bool isPersistent);
        ClaimsPrincipal? ValidateAccessToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<TokenResponse?> RefreshTokensAsync(string refreshToken);
    }
}
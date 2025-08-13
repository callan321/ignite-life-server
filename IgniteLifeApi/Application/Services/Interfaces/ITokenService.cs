using System.Security.Claims;
using IgniteLifeApi.Application.Dtos.Auth;

namespace IgniteLifeApi.Application.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(Guid userId, bool isPersistent, CancellationToken cancellationToken = default);
        ClaimsPrincipal? ValidateAccessToken(string token);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<TokenResponse?> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}

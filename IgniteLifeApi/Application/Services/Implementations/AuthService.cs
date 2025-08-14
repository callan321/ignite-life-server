using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Dtos.Common;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return ServiceResult<TokenResponse>.Unauthorized("Invalid email or password.");

            // Optional: block unconfirmed
            // if (!user.EmailConfirmed)
            //     return ServiceResult<TokenResponse>.Unauthorized("Email not confirmed.");

            var signIn = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!signIn.Succeeded)
                return ServiceResult<TokenResponse>.Unauthorized("Invalid email or password.");

            var tokenResponse = await _tokenService.GenerateTokensAsync(user.Id, request.RememberMe, cancellationToken);
            return ServiceResult<TokenResponse>.SuccessResult(tokenResponse);
        }

        public async Task<ServiceResult<Unit>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ServiceResult<Unit>.BadRequest("Refresh token is required.");

            await _tokenService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
            await _signInManager.SignOutAsync();
            return ServiceResult<Unit>.NoContentResult();
        }

        public async Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return ServiceResult<AuthStatusResponse>.NotFound("User not found.");

            var response = new AuthStatusResponse
            {
                UserId = user.Id,
                Email = user.Email
            };

            return ServiceResult<AuthStatusResponse>.SuccessResult(response);
        }

        public async Task<ServiceResult<Unit>> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var tokenResponse = await _tokenService.RefreshTokensAsync(refreshToken, cancellationToken);
            if (tokenResponse is null)
                return ServiceResult<Unit>.Unauthorized("Invalid or expired refresh token.");

            return ServiceResult<Unit>.SuccessResult(Unit.Value);
        }
    }
}

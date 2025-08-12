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
        private readonly UserManager<AdminUser> _userManager;
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<AdminUser> userManager,
            SignInManager<AdminUser> signInManager,
            ITokenService tokenService
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequest request)
        {
            // Find admin by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ServiceResult<TokenResponse>.Unauthorized("Invalid email or password.");

            // Validate password
            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true
            );
            if (!signInResult.Succeeded)
                return ServiceResult<TokenResponse>.Unauthorized("Invalid email or password.");

            // Generate JWT + Refresh token
            var tokenResponse = await _tokenService.GenerateTokensAsync(user, request.RememberMe);

            // Update user last login time
            await _userManager.UpdateAsync(user);

            return ServiceResult<TokenResponse>.SuccessResult(tokenResponse);
        }

        public async Task<ServiceResult<Unit>> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ServiceResult<Unit>.BadRequest("Refresh token is required.");

            await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            await _signInManager.SignOutAsync();

            return ServiceResult<Unit>.NoContentResult();
        }

        public Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Task.FromResult(ServiceResult<AuthStatusResponse>.NotFound("User not found."));

            var response = new AuthStatusResponse
            {
                UserId = user.Id,
                Email = user.Email,
            };

            return Task.FromResult(ServiceResult<AuthStatusResponse>.SuccessResult(response));
        }
    }
}

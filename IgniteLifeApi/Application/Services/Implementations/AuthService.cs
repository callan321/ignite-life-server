using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Dtos.Common;
using IgniteLifeApi.Application.Services.Common;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IgniteLifeApi.Application.Services.Implementations
{
    public class AuthService(
        UserManager<AdminUser> userManager,
        SignInManager<AdminUser> signInManager,
        ITokenService tokenService
        ) : IAuthService
    {
        private readonly UserManager<AdminUser> _userManager = userManager;
        private readonly SignInManager<AdminUser> _signInManager = signInManager;
        private readonly ITokenService _tokenService = tokenService;

        public Task<ServiceResult<Unit>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<AuthStatusResponse>> GetUserStatusAsync(Guid userId)
        {
            throw new NotImplementedException();
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

            // TODO - Implement RefreshToken entity and persist it in the database
            // Add tokens to db in user and on table   
            // user.RefreshTokens = tokenResponse.RefreshToken
            // user.RefreshTokens.Add();

            await _userManager.UpdateAsync(user);

            return ServiceResult<TokenResponse>.SuccessResult(tokenResponse);
        }


        public Task<ServiceResult<Unit>> LogoutAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Unit>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

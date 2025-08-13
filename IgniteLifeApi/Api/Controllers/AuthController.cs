using IgniteLifeApi.Api.Controllers.Common;
using IgniteLifeApi.Api.OpenApi.Attributes;
using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IgniteLifeApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwt;

        public AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions)
        {
            _authService = authService;
            _jwt = jwtOptions.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [ProducesAuthCookies]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        [HttpPost("logout")]
        [Authorize(Policy = "VerifiedUser")]
        [ProducesAuthCookies]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return NoContent();

            var result = await _authService.LogoutAsync(refreshToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        [HttpGet("status")]
        [Authorize(Policy = "VerifiedUser")]
        [RequiresAuthCookies]
        [ProducesAuthCookies]
        public async Task<IActionResult> GetAuthStatus()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idValue, out var userId))
                return Unauthorized();

            var result = await _authService.GetUserStatusAsync(userId);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [RequiresAuthCookies]
        [ProducesAuthCookies]
        public async Task<IActionResult> RefreshTokens()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return NoContent();

            var result = await _authService.RefreshTokensAsync(refreshToken);
            return ServiceResultToActionResult.ToActionResult(this, result);
        }

        private string? GetRefreshToken()
        {
            return Request.Cookies.TryGetValue(_jwt.RefreshTokenCookieName, out var token) && !string.IsNullOrWhiteSpace(token)
                ? token
                : null;
        }
    }
}

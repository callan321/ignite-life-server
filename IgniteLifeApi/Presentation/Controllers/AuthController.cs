using IgniteLifeApi.Application.Dtos.Auth;
using IgniteLifeApi.Application.Services.Interfaces;
using IgniteLifeApi.Presentation.OpenApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesAuthCookies]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] VerifyEmailRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesAuthCookies]
        public async Task<IActionResult> Logout()
        {
            throw new NotImplementedException();
        }

        [HttpGet("status")]
        [Authorize(Policy = "VerifiedUser")]
        [RequiresAuthCookies]
        [ProducesAuthCookies]
        public async Task<IActionResult> GetAuthStatus()
        {
            throw new NotImplementedException();
        }
    }
}
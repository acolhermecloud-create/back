using API.Payloads;
using Domain;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Util;

namespace API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(IAuthService authService,
        IUtilityService utilityService) : Controller
    {
        private readonly IAuthService _authService = authService;
        private readonly IUtilityService _utilityService = utilityService;

        [HttpPost("login/{email}/{password}/{userType}")]
        public async Task<IActionResult> Login(string email, string password, UserType userType)
        {
            try
            {
                var response = await _authService.AuthenticateUserWithPassword(email, password, userType);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex);
            }
        }

        [HttpPost("otp/auth")]
        [AllowAnonymous]
        public async Task<IActionResult> FinalizeAuthentication([FromBody] OtpAuthPayload payload)
        {
            try
            {
                var response = await _authService.FinalizeAuthentication(payload.Email, payload.Otp);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex);
            }
        }

        [HttpGet("generate/twofactor/image")]
        [Authorize]
        public async Task<IActionResult> CreateUrlTo2FA()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var response = await _authService.CreateUrlTo2FA(user.Id);

                return Ok(new { qrCode = response });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex);
            }
        }

        [HttpPost("save/2fa")]
        [Authorize]
        public async Task<IActionResult> Save2fa([FromBody] OtpAuthPayload payload)
        {
            try
            {
                var response = await _authService.Save2Fa(payload.Email, payload.Otp);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/2fa")]
        [Authorize]
        public async Task<IActionResult> Delete2fa()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var response = await _authService.Delete2Fa(user.Id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("recovery/{email}")]
        public async Task<IActionResult> RecoveryPassword(string email)
        {
            try
            {
                await _authService.RecoverPassword(email);
                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex);
            }
        }

        [HttpPost("validate/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            try
            {
                bool validation = _authService.ValidateToken(token);

                if (validation)
                    return Ok();
                else
                    return Unauthorized();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex);
            }
        }
    }
}

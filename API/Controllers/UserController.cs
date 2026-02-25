using API.Payloads;
using Domain;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Util;

namespace API.Controllers
{
    [ApiController]
    [Route("account")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IUtilityService _utilityService;
        private readonly ICampaignService _campaignService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, 
            IAuthService authService,
            IUtilityService utilityService,
            ICampaignService campaignService,
            IConfiguration configuration)
        {
            _userService = userService;
            _authService = authService;
            _utilityService = utilityService;
            _campaignService = campaignService;
            _configuration = configuration;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserPayload user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Name))
                    throw new Exception("Nome não pode ficar em branco");

                if (string.IsNullOrEmpty(user.Email) || !Functions.ValidateEmail(user.Email))
                    throw new Exception("Email informado é inválido");

                if (string.IsNullOrEmpty(user.Password))
                    throw new Exception("Senha não pode ficar em branco");

                if(!Functions.ValidateDocument(user.DocumentId))
                    throw new Exception("Numero do documento informado é inválido");

                var userType = user.Type == UserType.Admin ? UserType.Common : user.Type;

                await _userService.Create(
                    user.Email.ToLower(), user.Name, user.Password, user.DocumentId,
                    userType);

                var token = await _authService.AuthenticateUserWithPassword(user.Email.ToLower(), user.Password);

                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create/admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdmin([FromBody] UserPayload user)
        {
            try
            {
                var apiKey = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

                if (apiKey != _configuration["System:password"])
                {
                    if(string.IsNullOrEmpty(apiKey))
                        return Unauthorized();

                    var validation = _utilityService.ValidateJwtToken(apiKey);
                    if (!validation)
                        return Unauthorized();
                }

                if (string.IsNullOrEmpty(user.Name))
                    throw new Exception("Nome não pode ficar em branco");

                if (string.IsNullOrEmpty(user.Email) || !Functions.ValidateEmail(user.Email))
                    throw new Exception("Email informado é inválido");

                if (string.IsNullOrEmpty(user.Password))
                    throw new Exception("Senha não pode ficar em branco");

                await _userService.Create(
                    user.Email.ToLower(), user.Name, user.Password, user.DocumentId,
                    UserType.Admin);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserPayload user)
        {
            try
            {
                var parsed = Guid.TryParse(user.Id, out Guid parsedId);

                if (string.IsNullOrEmpty(user.Id) || !parsed)
                    throw new Exception("Informe o ID corretamente");

                if (string.IsNullOrEmpty(user.Name))
                    throw new Exception("Nome não pode ficar em branco");

                if (string.IsNullOrEmpty(user.Email))
                    throw new Exception("Email não pode ficar em branco");

                var response = await _userService.Update(parsedId, user.Email, user.Name, user.Phone);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("update/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordPayload payload)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = _utilityService.GetUserByToken(token);

                if (string.IsNullOrEmpty(payload.OldPassword))
                    throw new Exception("Informe a senha antiga corretamente");

                if (string.IsNullOrEmpty(payload.NewPassword))
                    throw new Exception("Informe a senha antiga corretamente");

                await _userService.UpdatePassword(user.Id, payload.OldPassword, payload.NewPassword);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("request/challenge")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestChallenge([FromBody] RequestChallengePayload payload)
        {
            try
            {
                await _userService.RequestChallege(payload.Reference);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("password/update/with/challenge")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdatePasswordWithChallange([FromBody] ForgotPasswordPayload payload)
        {
            try
            {
                await _userService.UpdatePasswordWithChallenge(payload.UserMail, payload.Challenge, payload.NewPassword);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/avatar/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAvatar(string id, IFormFile file)
        {
            try
            {

                bool parsed = Guid.TryParse(id, out Guid parsedId);

                if (!parsed) throw new Exception("Usuário inválido");

                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo encontrado");
                }

                var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png"};
                var fileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(fileName);

                if (!allowedExtensions.Contains(extension.ToLower()))
                {
                    return BadRequest("Arquivo não permitido");
                }

                Stream stream = file.OpenReadStream();
                var avatarUrl = await _userService.UpdateAvatar(parsedId, stream, extension);

                return Ok(avatarUrl);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("remove/avatar")]
        [Authorize]
        public async Task<IActionResult> RemoveAvatar()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = _utilityService.GetUserByToken(token);

                 await _userService.RemoveAvatar(user.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/address")]
        [Authorize]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressPayload payload)
        {
            try
            {
                bool parsed = Guid.TryParse($"{payload.UserId}", out Guid parsedId);

                if (!parsed) throw new Exception("Usuário inválido");

                await _userService.UpdateAddress(parsedId, payload.Street,
                    payload.City, payload.State, payload.ZipCode, payload.Country);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("search/{name}")]
        [Authorize("Admin")]
        public async Task<IActionResult> SearchByName(string name)
        {
            try
            {
                var result = await _userService.SearchByName(name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("get")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetAll([FromBody] GetUsersFilterPayload payload)
        {
            try
            {
                var users = await _userService.ListAll(payload.Name, payload.Type, payload.Page, payload.PageSize);

                return Ok(new { users });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("change/status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus([FromBody] UserChangeStatusPayload payload)
        {
            try
            {
                var parsed = Guid.TryParse(payload.UserId, out Guid parsedId);

                if (!parsed) throw new Exception("Usuário não encontrado");

                await _userService.ChangeStatus(parsedId, payload.UserStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get/users/points/{page}/{pageSize}/{moreThan}")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetUsersWhoHavePointsGreaterThan(int page, int pageSize, int moreThan)
        {
            try
            {
                var result = await _userService.GetUsersWhoHavePointsGreaterThan(moreThan, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("balanceAwaitingRelease")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceAwaitingRelease()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = _utilityService.GetUserByToken(token);

                var balance = await _campaignService.GetUserBalanceAwaitingRelease(user.Id);

                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("balanceReleasedForWithdraw")]
        [Authorize]
        public async Task<IActionResult> GetUserBalanceReleasedForWithdraw()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = _utilityService.GetUserByToken(token);

                var balance = await _campaignService.GetUserBalanceReleasedForWithdraw(user.Id);

                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}

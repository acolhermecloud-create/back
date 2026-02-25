using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("ong")]
    public class OngController : Controller
    {
        private readonly IUserService _userService;

        public OngController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        [Authorize("admin")]
        public async Task<IActionResult> Create([FromForm] OngPayload payload)
        {
            try
            {
                var parsedCategoryId = Guid.TryParse(payload.CategoryId, out Guid categoryId);
                var parsedOwnerId = Guid.TryParse(payload.OwnerId, out Guid ownerId);

                if (!parsedCategoryId) throw new Exception("Categoria inválido");
                if (!parsedOwnerId) throw new Exception("Responsável inválido");

                if (payload.Banner == null || payload.Banner.Length == 0)
                    return BadRequest("Nenhum arquivo encontrado");

                var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                var fileName = Path.GetFileName(payload.Banner.FileName);
                var extension = Path.GetExtension(fileName);

                if (!allowedExtensions.Contains(extension.ToLower()))
                    return BadRequest("Arquivo não permitido");

                Stream stream = payload.Banner.OpenReadStream();
                Dictionary<Stream, string> banner = new() { { stream, extension } };

                await _userService.CreateOng(
                    ownerId,
                    categoryId,
                    payload.Name,
                    payload.Description,
                    payload.About,
                    payload.Site,
                    payload.Mail,
                    payload.Phone,
                    payload.Instagram,
                    payload.Youtube,
                    banner,
                    payload.Street,
                    payload.City,
                    payload.State,
                    payload.ZipCode,
                    payload.Country);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/{size}/{pageSize}")]
        [Authorize("admin")]
        public async Task<IActionResult> GetAll(int size, int pageSize)
        {
            try
            {
                var result = await _userService.GetOngs(size, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update")]
        [Authorize("admin")]
        public async Task<IActionResult> Update([FromBody] OngPayload payload)
        {
            try
            {
                var parsedCategoryId = Guid.TryParse(payload.CategoryId, out Guid categoryId);
                var parsedId = Guid.TryParse(payload.Id, out Guid id);

                if (!parsedCategoryId) throw new Exception("Categoria inválido");
                if (!parsedId) throw new Exception("Id inválido");

                await _userService.UpdateOng(
                    id,
                    categoryId,
                    payload.Name,
                    payload.Description,
                    payload.About,
                    payload.Site,
                    payload.Mail,
                    payload.Phone,
                    payload.Instagram,
                    payload.Youtube,
                    payload.Street,
                    payload.City,
                    payload.State,
                    payload.ZipCode,
                    payload.Country);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/banner/{id}")]
        [Authorize("admin")]
        public async Task<IActionResult> UpdateBanner(string id, IFormFile file)
        {
            try
            {
                var parsedId = Guid.TryParse(id, out Guid ongId);

                if (file == null || file.Length == 0)
                    return BadRequest("Nenhum arquivo encontrado");

                var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                var fileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(fileName);

                if (!allowedExtensions.Contains(extension.ToLower()))
                    return BadRequest("Arquivo não permitido");

                Stream stream = file.OpenReadStream();

                await _userService.UpdateOngBanner(ongId, stream,extension);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}

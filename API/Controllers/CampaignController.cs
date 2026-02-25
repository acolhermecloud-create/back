using API.Payloads;
using Domain;
using Domain.Interfaces.Services;
using Domain.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("campaign")]
    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IUtilityService _utilityService;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IFakeDataForCampaignService _fakeDataForCampaignService;
        private readonly IConfiguration _configuration;

        public string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

        public CampaignController(
            ICampaignService campaignService,
            IUtilityService utilityService,
            IUserService userService,
            IFakeDataForCampaignService fakeDataForCampaignService,
            IConfiguration configuration,
            IAuthService authService)
        {
            _campaignService = campaignService;
            _utilityService = utilityService;
            _userService = userService;
            _configuration = configuration;
            _authService = authService;

            _fakeDataForCampaignService = fakeDataForCampaignService;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromForm] CampaignPayload payload)
        {
            Guid creatorId = Guid.Empty;

            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

                if (string.IsNullOrEmpty(token))
                {
                    if (string.IsNullOrEmpty(payload.Email))
                        throw new Exception("Email não pode ficar em branco");

                    if (string.IsNullOrEmpty(payload.Name))
                        throw new Exception("Nome não pode ficar em branco");

                    if (string.IsNullOrEmpty(payload.Password))
                        throw new Exception("Senha não pode ficar em branco");

                    if (string.IsNullOrEmpty(payload.DocumentId))
                        throw new Exception("Identidade não pode ficar em branco");

                    var userId = await _userService.GetOrCreate(
                        payload.Email,
                        payload.Name,
                        payload.Password,
                        payload.DocumentId,
                        UserType.Common);

                    Guid.TryParse(userId, out Guid parsedCreatorId);
                    creatorId = parsedCreatorId;
                }
                else
                {
                    var user = _utilityService.GetUserByToken(token);
                    creatorId = user.Id;
                }

                if (string.IsNullOrEmpty(payload.CategoryId))
                    throw new Exception("Informe a categoria");

                if (string.IsNullOrEmpty(payload.Title))
                    throw new Exception("Titulo não pode ficar em branco");

                if (string.IsNullOrEmpty(payload.Description))
                    throw new Exception("Descrição não pode ficar em branco");

                if (payload.FinancialGoal < int.Parse(_configuration["System:MinimalFinancialGoal"]))
                    throw new Exception($"O valor mínimo para a vaquinha é R$ {_configuration["System:MinimalFinancialGoal"]}");

                if (payload.Deadline < 7)
                    payload.Deadline = 7;

                if (payload.Deadline > 120)
                    payload.Deadline = 120;

                var validAt = DateTime.Now.AddDays(payload.Deadline);

                bool parsedCategoryId = Guid.TryParse(payload.CategoryId, out Guid categoryId);

                Dictionary<Stream, string> files = [];
                foreach (var image in payload.Files)
                {
                    if (image == null || image.Length == 0)
                        continue;

                    var fileName = Path.GetFileName(image.FileName);
                    var extension = Path.GetExtension(fileName);

                    Stream stream = image.OpenReadStream();
                    files.Add(stream, extension);
                }

                if (files.Values.Any(ext => !AllowedExtensions.Contains(ext.ToLower())))
                    throw new Exception("Extensão não permitida");

                var campaign = await _campaignService.Create(payload.Title, payload.Description,
                    payload.FinancialGoal, payload.BeneficiaryName ?? string.Empty, payload.CampaignisForWho,
                    validAt,
                    categoryId,
                    creatorId,
                    files);

                if (string.IsNullOrEmpty(token))
                {
                    var response = await _authService.AuthenticateUserWithPassword(payload.Email, payload.Password);
                    return Ok(new { login = response, campaign });
                }

                return Ok(new { campaign });
            }
            catch (Exception ex)
            {
                _ = _userService.Delete(creatorId);
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromForm] CampaignUpdatePayload payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload.Id))
                    throw new Exception("Informe o Id");

                if (string.IsNullOrEmpty(payload.CategoryId))
                    throw new Exception("Informe a categoria");

                if (string.IsNullOrEmpty(payload.Title))
                    throw new Exception("Titulo não pode ficar em branco");

                if (string.IsNullOrEmpty(payload.Description))
                    throw new Exception("Descrição não pode ficar em branco");

                if (payload.FinancialGoal < int.Parse(_configuration["System:MinimalFinancialGoal"]))
                    throw new Exception($"O valor mínimo para a vaquinha é R$ {_configuration["System:MinimalFinancialGoal"]}");

                var campaignId = Guid.Parse(payload.Id);
                var categoryId = Guid.Parse(payload.CategoryId);

                if (payload.Deadline < 7)
                    payload.Deadline = 7;

                if (payload.Deadline > 120)
                    payload.Deadline = 120;

                var validAt = DateTime.Now.AddDays(payload.Deadline);

                var newCampaign = await _campaignService.Update(
                    campaignId,
                    payload.Title,
                    payload.Description,
                    payload.FinancialGoal,
                    validAt,
                    categoryId);

                return Ok(newCampaign);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/status")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateCampaignStatusPayload payload)
        {
            try
            {
                var campaignId = Guid.Parse(payload.Id);
                await _campaignService.UpdateStatus(campaignId, payload.Status, payload.Reason);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/listing")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateListing([FromBody] UpdateListingCampaignPayload payload)
        {
            try
            {
                var campaignId = Guid.Parse(payload.CampaignId);
                await _campaignService.UpdateListingCampaign(campaignId, payload.Listing);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("desactive/{id}")]
        [Authorize]
        public async Task<IActionResult> Desactive(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Informe o Id");

                var campaignId = Guid.Parse(id);

                await _campaignService.UpdateStatus(campaignId, CampaignStatus.Inactive, null);

                return Ok(new { status = true });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/{id}/{page}/{pageSize}")]
        public async Task<IActionResult> Get(string id, int page, int pageSize)
        {
            try
            {
                var campaignId = Guid.TryParse(id, out Guid parsedId);

                if (!campaignId)
                    throw new Exception("Informe o Id corretamente");

               var campaign = await _campaignService.GetById(parsedId, page, pageSize);

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/by/donor/{donorId}/{page}/{pageSize}")]
        public async Task<IActionResult> GetByDonor(string donorId, int page, int pageSize)
        {
            try
            {
                var parsed = Guid.TryParse(donorId, out Guid parsedId);

                if (!parsed)
                    throw new Exception("Informe o Id corretamente");

                var campaign = await _campaignService.GetCampaignsByDonor(parsedId, page, pageSize);

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/donations/{id}/{page}/{pageSize}")]
        public async Task<IActionResult> GetDonationsByCampanign(string id, int page, int pageSize)
        {
            try
            {
                var parsed = Guid.TryParse(id, out Guid parsedId);

                if (!parsed)
                    throw new Exception("Informe o Id corretamente");

                var campaign = await _campaignService.GetDonationsByCampaign(parsedId, page, pageSize);

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("filter")]
        public async Task<IActionResult> GetByFilter([FromBody] CampaignFilterPayload payload)
        {
            try
            {
                var campaignId = Guid.TryParse(payload.CategoryId, out Guid parsedId);

                List<Guid> guids = [];
                if(payload.Guids != null)
                {
                    foreach (var guid in payload.Guids)
                    {
                        bool parsed = Guid.TryParse(guid, out Guid parsedGuid);
                        if (parsed) guids.Add(parsedGuid);
                    }
                }

                var campaign = await _campaignService.GetFilteredCampaigns(
                    payload.StartDate,
                    payload.EndDate,
                    campaignId ? parsedId : null,
                    payload.Name,
                    guids,
                    payload.Listing,
                    payload.Status ?? Domain.CampaignStatus.Active,
                    payload.Page,
                    payload.PageSize);

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("only/filter")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetByOnlyFilters([FromBody] CampaignFilterPayload payload)
        {
            try
            {
                var campaign = await _campaignService.GetFilteredCampaigns(
                    payload.StartDate,
                    payload.EndDate,
                    payload.Name,
                    payload.Status,
                    payload.Listing,
                    payload.Page,
                    payload.PageSize);

                return Ok(campaign);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("get/by/user/{page}/{pageSize}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(int page, int pageSize)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var campaigns = await _campaignService.GetByUserId(user.Id, page, pageSize);

                return Ok(new { campaigns });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            try
            {
                var result = await _campaignService.GetBySlug(slug);

                return Ok(new { campaign = result });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }
        
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var campaignId = Guid.TryParse(id, out Guid parsedId);

                if (!campaignId)
                    throw new Exception("Informe o Id corretamente");

                await _campaignService.Delete(parsedId);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("update/images/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateImages(string id, [FromForm] List<IFormFile> files)
        {
            try
            {
                var campaignId = Guid.TryParse(id, out Guid parsedId);

                if (!campaignId)
                    throw new Exception("Informe o Id corretamente");

                Dictionary<Stream, string> images = [];
                foreach (var image in files)
                {
                    if (image == null || image.Length == 0)
                        continue;

                    var fileName = Path.GetFileName(image.FileName);
                    var extension = Path.GetExtension(fileName);

                    Stream stream = image.OpenReadStream();
                    images.Add(stream, extension);
                }

                if (images.Values.Any(ext => !AllowedExtensions.Contains(ext.ToLower())))
                    throw new Exception("Extensão não permitida");

                var imageKeys = await _campaignService.AddImages(parsedId, images);

                return Ok(imageKeys);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("delete/images")]
        [Authorize]
        public async Task<IActionResult> RemoveImages([FromBody] DeleteImagesPayload deleteImagesPayload)
        {
            try
            {
                var campaignId = Guid.TryParse(deleteImagesPayload.CampaignId, out Guid parsedId);

                if (!campaignId)
                    throw new Exception("Informe o Id corretamente");

                await _campaignService.RemoveImages(parsedId, [.. deleteImagesPayload.ImagesKeys]);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("report")]
        public async Task<IActionResult> Report([FromBody] ReportPayload payload)
        {
            try
            {
                var parsedCampaignId = Guid.TryParse(payload.CampaignId, out Guid campaignId);

                if (!parsedCampaignId)
                    throw new Exception("Informe o ID da campanha");

                Guid? denunciatorId = null;

                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

                if (!string.IsNullOrEmpty(token))
                {
                    var user = _utilityService.GetUserByToken(token);
                    denunciatorId = user.Id;
                }

                await _campaignService.Report(
                    campaignId,
                    denunciatorId,
                    payload.IAm,
                    payload.ARespectFor,
                    payload.Why,
                    payload.Description);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("delete/report/{id}")]
        [Authorize("admin")]
        public async Task<IActionResult> RemoveReport(string id)
        {
            try
            {
                var parsed = Guid.TryParse(id, out Guid reportId);

                if (!parsed)
                    throw new Exception("Informe o Id corretamente");

                await _campaignService.RemoveReportById(reportId);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("report/update")]
        [Authorize("admin")]
        public async Task<IActionResult> ReportUpdate([FromBody] ReportPayload payload)
        {
            try
            {
                var parsedReportId = Guid.TryParse(payload.ReportId, out Guid reportId);

                if (!parsedReportId)
                    throw new Exception("Informe o ID da denuncia");

                await _campaignService.UpdateReportById(
                    reportId,
                    payload.IAm,
                    payload.ARespectFor,
                    payload.Why,
                    payload.Description);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/reports/{page}/{pageSize}")]
        [Authorize("admin")]
        public async Task<IActionResult> GetReports(int page, int pageSize)
        {
            try
            {
                var reports = await _campaignService.GetAllReports(page, pageSize);

                return Ok(reports);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _campaignService.GetAllCategory();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("donation/pix/create")]
        public async Task<IActionResult> CreateDonationIntentPix([FromBody] DonationIntentPayload donationIntentPayload)
        {
            try
            {
                // Obtendo o IP do cliente
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP não disponível";

                DonationRequest request = new()
                {
                    CampaignId = donationIntentPayload.CampaignId,
                    DonorEmail = donationIntentPayload.DonorEmail,
                    DonorName = donationIntentPayload.DonorName,
                    DonorDocumentId = donationIntentPayload.DonorDocumentId,
                    DonorPhone = donationIntentPayload.DonorPhone.Replace("+", ""),
                    DonationType = DonationType.Money,
                    TransationMethod = TransationMethod.Cash,
                    Value = donationIntentPayload.Value,
                    Amount = 1,
                    DonateAt = DateTime.Now,
                    ClientIp = clientIp
                };

                var response = await _campaignService.GeneratePaymentData(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("payment/check/{transactionId}")]
        public async Task<IActionResult> ConfirmDonationIntentPix(string transactionId)
        {
            try
            {
                var payed = await _campaignService.CheckDonation(transactionId);

                return Ok(new { payed });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("donation/small/digitalStickers")]
        public async Task<IActionResult> MakeDonationDigitalStickers([FromBody] SmallDonationPayload smallDonation)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                await _campaignService.MakeSmallDonation(smallDonation.CampaignId,
                    user.Id,
                    DonationType.SmallDonations,
                    smallDonation.Quantity);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("logs/{campaignId}")]
        public async Task<IActionResult> GetLogs(string campaignId)
        {
            try
            {
                var parsed = Guid.TryParse(campaignId, out Guid id);
                if (!parsed)
                    throw new Exception("Id inválido");

                var logs = await _campaignService.GetLogs(id);

                return Ok(new { logs });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("plans")]
        public async Task<IActionResult> ListPlans()
        {
            try
            {
                var plans = await _campaignService.ListPlans();

                return Ok(new { plans });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("add/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] CommentPayload payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload.CampaignId))
                    throw new Exception("Id da campanha não pode ficar em branco");

                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                Guid.TryParse(payload.CampaignId, out Guid parsedCampaignId);
                await _campaignService.AddComment(parsedCampaignId, user.Id, payload.Comment);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpDelete("remove/comment/{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveComment(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Id não pode ficar em branco");

                Guid.TryParse(id, out Guid commentId);

                await _campaignService.RemoveComment(commentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("addMock")]
        public async Task<IActionResult> AddMock([FromBody] AddMockPayload payload)
        {
            try
            {
                if (_configuration["System:password"] != payload.Password)
                    return Unauthorized();

                await _fakeDataForCampaignService.CreateForCampaign(payload.CampaignSlug, 
                    payload.AllowDonations, payload.Goal);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("record/utm")]
        public async Task<IActionResult> RecordUtm([FromBody] Utm utm)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP não disponível";

                if (utm?.Customer != null)
                    utm.Customer.Ip = clientIp;

                await _campaignService.RecordUtm(utm!);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }
    }
}
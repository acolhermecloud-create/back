using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("leverage")]
    public class LeverageRequestController : Controller
    {
        private readonly ILeverageRequestService _leverageRequestService;
        private readonly IUtilityService _utilityService;

        public LeverageRequestController(ILeverageRequestService leverageRequestService,
            IUtilityService utilityService)
        {
            _leverageRequestService = leverageRequestService;
            _utilityService = utilityService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] LeveragePayload payload)
        {
            Guid creatorId = Guid.Empty;

            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);
                creatorId = user.Id;

                if (string.IsNullOrEmpty(payload.CampaignId))
                    throw new Exception("Informe o id da campanha");

                Guid.TryParse(payload.CampaignId, out Guid parsedCampaignId);
                Guid.TryParse(payload.PlanId, out Guid parsedPlanId);

                Dictionary<Stream, string> files = [];
                if (payload.Files != null)
                {
                    foreach (var image in payload.Files)
                    {
                        if (image == null || image.Length == 0)
                            continue;

                        var fileName = Path.GetFileName(image.FileName);
                        var extension = Path.GetExtension(fileName);

                        Stream stream = image.OpenReadStream();
                        files.Add(stream, extension);
                    }
                }

                await _leverageRequestService.Record(
                    parsedCampaignId,
                    creatorId, parsedPlanId, payload.Phone, payload.HasSharedCampaign, payload.EvidenceLinks,
                    payload.WantsToBoostCampaign, payload.PreferredContactMethod, files);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("list/{page}/{size}")]
        [Authorize("Admin")]
        public async Task<IActionResult> List(int page, int size)
        {
            try
            {
                var leverages = await _leverageRequestService.GetAll(page, size);

                return Ok(new  { leverages });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("update/status")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] LeverageRequestUpdateStatusPayload payload)
        {
            try
            {
                await _leverageRequestService.ChangeStatus(payload.Id, payload.Status);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }
    }
}

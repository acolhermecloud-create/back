using API.Payloads;
using Domain;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service;

namespace API.Controllers
{
    [ApiController]
    [Route("plan")]
    public class PlansController : Controller
    {
        private readonly IUtilityService _utilityService;
        private readonly IPlanService _planService;

        public PlansController(IUtilityService utilityService,
            IPlanService planService)
        {
            _utilityService = utilityService;
            _planService = planService;
        }

        [HttpGet("list/{page}/{size}")]
        [Authorize("Admin")]
        public async Task<IActionResult> List(int page, int size)
        {
            try
            {
                var plans = await _planService.ListPlans(page, size);

                return Ok(new { plans });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("update")]
        [Authorize("Admin")]
        public async Task<IActionResult> Update([FromBody] RecordPlanPayload payload)
        {
            try
            {
                var parse = Guid.TryParse(payload.Id, out Guid parsedPlanId);

                if (!parse)
                    return BadRequest("Id não pode ficar nulo");

                await _planService.UpdatePlan(parsedPlanId,
                    payload.Title,
                    payload.Description,
                    payload.Benefits,
                    payload.PercentToBeCharged,
                    payload.FixedRate,
                    payload.NeedApproval,
                    payload.Default,
                    payload.Active);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }
    }
}

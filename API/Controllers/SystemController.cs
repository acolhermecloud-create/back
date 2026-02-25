using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("system")]
    public class SystemController(ITransfeeraService transfeeraService,
        IConfiguration configuration,
        ISystemService systemService) : Controller
    {
        private readonly ITransfeeraService _transfeeraService = transfeeraService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ISystemService _systemService = systemService;

        [HttpPost("set-cashin-transfeera-webhook")]
        public async Task<IActionResult> SetTransfeeraCashInWebhook()
        {
            try
            {
                string webhookUrl = $"{_configuration["System:BackendUrl"]}/webhook/payment/confirm";

                await _transfeeraService.SetTransfeeraWebhook(["CashIn"], webhookUrl);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("set-cashout-transfeera-webhook")]
        public async Task<IActionResult> SetTransfeeraCashOutWebhook()
        {
            try
            {

                string webhookUrl = $"{_configuration["System:BackendUrl"]}/webhook/withdraw/transfeera/confirm";

                await _transfeeraService.SetTransfeeraWebhook(["Transfer"], webhookUrl);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetSystemConfiguration()
        {
            try
            {
                var config = await _systemService.GetGatewayConfiguration();
                return Json(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("set/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> SetSystemConfiguration([FromBody] SystemConfigurationPayload payload)
        {
            try
            {
                await _systemService.UpdateConfiguration(payload.Pix, payload.Card, payload.TryToGenerateCashInInOtherAcquirers);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get/reflow/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetReflowConfiguration()
        {
            try
            {
                var config = await _systemService.GetReflowPayData();
                return Json(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get/reflow-v2/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetReflowV2Configuration()
        {
            try
            {
                var config = await _systemService.GetReflowPayV2Data();
                return Json(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get/bloobank/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetBloobankConfiguration()
        {
            try
            {
                var config = await _systemService.GetBloobankData();
                return Json(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get/transfeera/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetTransfeeraConfiguration()
        {
            try
            {
                var config = await _systemService.GetTransfeeraData();
                return Json(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update/reflow/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateReflowConfiguration([FromBody] FeesPayload payload)
        {
            try
            {
                await _systemService.UpdateReflowPayData(payload.Fixed, payload.Variable);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update/reflow-v2/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> UpdateReflowV2Configuration([FromBody] FeesPayload payload)
        {
            try
            {
                await _systemService.UpdateReflowPayV2Data(payload.Fixed, payload.Variable);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update/bloobank/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetBloobankConfiguration([FromBody] FeesPayload payload)
        {
            try
            {
                await _systemService.UpdateBloobankData(payload.Fixed, payload.Variable);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update/transfeera/acquirer/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetTransfeeraConfiguration([FromBody] FeesPayload payload)
        {
            try
            {
                await _systemService.UpdateTransfeeraAcquirerData(payload.Fixed, payload.Variable);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

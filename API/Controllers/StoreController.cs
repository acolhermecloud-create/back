using API.Payloads;
using Domain.Interfaces.Services;
using Domain.Objects.ReflowPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Service;
using Util;

namespace API.Controllers
{
    [ApiController]
    [Route("store")]
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IUtilityService _utilityService;

        public StoreController(IStoreService storeService, 
            IUtilityService utilityService)
        {
            _storeService = storeService;
            _utilityService = utilityService;
        }

        [HttpGet("get/digitalStickers")]
        public async Task<IActionResult> GetDigitalStickers()
        {
            try
            {
                var digitalStickers = await _storeService.GetAllDigitalStickers();

                return Ok(new { digitalStickers });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("buy/digitalStickers")]
        [Authorize]
        public async Task<IActionResult> BuyDigitalStickers([FromBody] BuyDigitalStickerPayload payload)
        {
            try
            {
                var parsed = Guid.TryParse(payload.PlanId, out Guid parsedId);
                if (string.IsNullOrEmpty(payload.PlanId) || !parsed)
                    throw new Exception("Informe o ID corretamente");

                Guid? campaingId = null;
                if (!string.IsNullOrEmpty(payload.CampaignId))
                    campaingId = Guid.Parse(payload.CampaignId);

                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP não disponível";

                var transactionData = await _storeService.AddToCartDigitalStickers(
                    campaingId, user.Id, parsedId, payload.Qtd, clientIp);

                return Ok(new { transactionData });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("payment/check/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDonationIntentPix(string transactionId)
        {
            try
            {
                var payed = await _storeService.CheckPaymentDigitalStickers(transactionId);

                return Ok(new { payed });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("get/user/digitalStickers")]
        [Authorize]
        public async Task<IActionResult> GetUserDigitalStickers()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var digitalStickers = await _storeService.GetUserDigitalStickers(user.Id);

                return Ok(new { digitalStickers });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("get/user/usage/digitalStickers")]
        [Authorize]
        public async Task<IActionResult> GetUserDigitalStickersUsage()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var digitalStickers = await _storeService.GetUserDigitalStickersUsage(user.Id);

                return Ok(new { digitalStickers });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}

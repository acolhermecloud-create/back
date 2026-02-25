using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("bank")]
    public class BankController : Controller
    {
        private readonly IBankService _bankService;
        private readonly IPaymentService _paymentService;
        private readonly IUtilityService _utilityService;

        public BankController(IBankService bankService, IUtilityService utilityService,
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
            _bankService = bankService;
            _utilityService = utilityService;
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawPayload payload)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                await _bankService.RequestWithDraw(user.Id, payload.Value);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("get/transactions/{page}/{pageSize}")]
        public async Task<IActionResult> ListTransactions(int page, int pageSize)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var transactions = await _bankService.ListCashOutTransactions(user.Id, page, pageSize);

                return Ok(new { transactions });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("get/balance")]
        public async Task<IActionResult> GetBalance(int page, int pageSize)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
                var user = _utilityService.GetUserByToken(token);

                var balance = await _bankService.GetBalance(user.Id);

                return Ok(new { balance });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpGet("baas/get/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetBAASConfiguration()
        {
            try
            {
                var baasConfig = await _bankService.GetBAASConfiguration();

                return Ok(new { baasConfig });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("baas/set/configuration")]
        [Authorize("Admin")]
        public async Task<IActionResult> SetBAASConfiguration([FromBody] BAASConfigurationPayload payload)
        {
            try
            {
                await _bankService.SetBAASConfiguration(payload.AnalyseWithdraw, 
                    payload.DailyWithdrawalLimitValue, payload.DailyWithdrawalMinimumValue);

                return Ok(new { message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("get/transaction")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetTransactions([FromBody] GetTransactionsPayload payload)
        {
            try
            {
                var transactions = await _bankService.ListTransactions(
                    payload.Statuses,
                    payload.Types,
                    payload.Page,
                    payload.PageSize);

                return Ok(new { transactions });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }

        [HttpPost("transaction/update")]
        [Authorize("Admin")]
        public async Task<IActionResult> Withdraw([FromBody] TransactionUpdateStatusPayload payload)
        {
            try
            {
                var parsed = Guid.TryParse(payload.TransactionId, out Guid parsedId);

                await _bankService.MakeWithDraw(parsedId, payload.Status);

                return Ok(new { message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("transaction/refund/{id}")]
        [Authorize("Admin")]
        public async Task<IActionResult> Refund(string id)
        {
            try
            {
                var parsed = Guid.TryParse(id, out Guid parsedId);

                await _bankService.RefundTransaction(parsedId);

                return Ok(new { message = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

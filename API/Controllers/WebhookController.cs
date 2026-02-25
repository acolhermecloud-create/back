using Domain.Bank;
using Domain.Interfaces.Services;
using Domain.Objects;
using Domain.Objects.Bloobank;
using Domain.Objects.ReflowPay;
using Domain.Objects.ReflowPayV2;
using Domain.Objects.Transfeera;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController(
        IPaymentService paymentService,
        IBankService bankService,
        ICampaignService campaignService,
        ITransfeeraService transfeeraService,
        IStoreService storeService,
        ICacheService cacheService) : Controller
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly ITransfeeraService _transfeeraService = transfeeraService;
        private readonly IBankService _bankService = bankService;
        private readonly ICampaignService _campaignService = campaignService;
        private readonly IStoreService _storeService = storeService;
        private readonly ICacheService _cacheService = cacheService;

        [HttpPost("payment/confirm")]
        public async Task<IActionResult> PaymentConfirm()
        {
            try
            {
                Request.EnableBuffering();
                using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                string rawPayload = await reader.ReadToEndAsync();
                dynamic payload = JsonConvert.DeserializeObject<dynamic>(rawPayload);

                if (payload == null)
                    return BadRequest(new { message = "Payload inválido." });

                // Convertendo para JSON
                var json = JsonConvert.SerializeObject(payload);

                var jsonDynamic = JsonConvert.DeserializeObject<dynamic>(json);

                if (jsonDynamic?.test != null && jsonDynamic?.test == true)
                    return Ok();

                // Tenta desserializar como ReflowpayV2
                ReflowPayV2Webhook? reflowPayV2Payload = JsonConvert.DeserializeObject<ReflowPayV2Webhook>(json);
                if (reflowPayV2Payload != null && reflowPayV2Payload?.Status != null && (reflowPayV2Payload?.Status == "paid"))
                {
                    var cachedPayment = await _cacheService.Get(reflowPayV2Payload.TransactionId.ToString())
                        ?? throw new Exception("Pagamento não encontrado.");

                    await _cacheService.Delete(reflowPayV2Payload.TransactionId.ToString());

                    TransactionData data = JsonConvert.DeserializeObject<TransactionData>(cachedPayment);

                    if (data.TransactionSource == Domain.BankTransactionSource.Campaign)
                        await _campaignService.ConfirmDonation(reflowPayV2Payload.TransactionId.ToString());
                    else
                        await _storeService.ConfirmPaymentDigitalStickers(reflowPayV2Payload.TransactionId.ToString());

                    return Ok();
                }

                // Tenta desserializar como Reflowpay
                var reflowpayPayload = JsonConvert.DeserializeObject<ReflowpayTransactionWebhook>(json);
                if (reflowpayPayload != null && reflowpayPayload?.OrderId != null && (reflowpayPayload?.Status == "paid" || reflowpayPayload?.Status == "received"))
                {
                    var cachedPayment = await _cacheService.Get(reflowpayPayload.OrderId) 
                        ?? throw new Exception("Pagamento não encontrado.");

                    await _cacheService.Delete(reflowpayPayload.OrderId);

                    TransactionData data = JsonConvert.DeserializeObject<TransactionData>(cachedPayment);

                    if(data.TransactionSource == Domain.BankTransactionSource.Campaign)
                        await _campaignService.ConfirmDonation(reflowpayPayload.OrderId);
                    else
                        await _storeService.ConfirmPaymentDigitalStickers(reflowpayPayload.OrderId);

                    return Ok();
                }

                // Tenta desserializar como Bloobank
                var bloobankPayload = JsonConvert.DeserializeObject<BloobankTransactionWebhook>(json);
                if (bloobankPayload?.Body?.Id != null && bloobankPayload.Body.Status == "approved")
                {
                    var cachedPayment = await _cacheService.Get(bloobankPayload.Body.Id)
                        ?? throw new Exception("Pagamento não encontrado.");

                    await _cacheService.Delete(bloobankPayload.Body.Id);

                    TransactionData data = JsonConvert.DeserializeObject<TransactionData>(cachedPayment);

                    if (data.TransactionSource == Domain.BankTransactionSource.Campaign)
                        await _campaignService.ConfirmDonation(bloobankPayload.Body.Id);
                    else
                        await _storeService.ConfirmPaymentDigitalStickers(bloobankPayload.Body.Id);

                    return Ok();
                }

                TransfeeraWebhook transfeeraPayload = JsonConvert.DeserializeObject<TransfeeraWebhook>(rawPayload);
                if(transfeeraPayload != null && transfeeraPayload.Object == "CashIn")
                {
                    var transfeeraSignature = Request.Headers["Transfeera-Signature"].ToString();

                    var validateSignature = await _transfeeraService.ValidateSignature(transfeeraSignature, rawPayload, "CashIn");
                    if (!validateSignature) return Unauthorized("Signature not recognized.");

                    var cachedPayment = await _cacheService.Get(transfeeraPayload.Data.IntegrationId)
                        ?? throw new Exception("Pagamento não encontrado.");

                    await _cacheService.Delete(transfeeraPayload.Data.IntegrationId);

                    TransactionData data = JsonConvert.DeserializeObject<TransactionData>(cachedPayment);

                    if (data.TransactionSource == Domain.BankTransactionSource.Campaign)
                        await _campaignService.ConfirmDonation(transfeeraPayload.Data.IntegrationId);
                    else
                        await _storeService.ConfirmPaymentDigitalStickers(transfeeraPayload.Data.IntegrationId);

                    return Ok();
                }

                return BadRequest(new { message = "Formato de payload desconhecido." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("withdraw/transfeera/confirm")]
        public async Task<IActionResult> WithdrawTransfeeraConfirm()
        {
            try
            {
                Request.EnableBuffering();
                using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                string rawPayload = await reader.ReadToEndAsync();
                dynamic payload = JsonConvert.DeserializeObject<dynamic>(rawPayload);

                if (payload == null)
                    return BadRequest(new { message = "Payload inválido." });

                // Convertendo para JSON
                var json = JsonConvert.SerializeObject(payload);

                if (payload?.test != null && payload?.test == true)
                    return Ok();

                var transfeeraSignature = Request.Headers["Transfeera-Signature"].ToString();

                var validateSignature = await _transfeeraService.ValidateSignature(transfeeraSignature, rawPayload, "Transfer");
                if (!validateSignature) return Unauthorized("Signature not recognized.");

                TransfeeraWebhook transfeeraPayload = JsonConvert.DeserializeObject<TransfeeraWebhook>(rawPayload);

                if(transfeeraPayload.Object == "Transfer" && transfeeraPayload.Data.Status == "FINALIZADO")
                {
                    var cachedTransaction = await _cacheService.Get($"{transfeeraPayload.Data.Id}") 
                        ?? throw new Exception("Transação não encontrada");

                    await _cacheService.Delete($"{transfeeraPayload.Data.Id}");

                    var bankTransaction = JsonConvert.DeserializeObject<BankTransaction>(cachedTransaction);
                    await _bankService.UpdateTransactionStatus(bankTransaction.Id, Domain.BankTransactionStatus.Completed);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

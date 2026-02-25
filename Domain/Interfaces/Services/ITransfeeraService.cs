using Domain.Objects.Transfeera;

namespace Domain.Interfaces.Services
{
    public interface ITransfeeraService
    {
        Task SetTransfeeraWebhook(string[] events, string webhookUrl);

        Task<bool> ValidateSignature(string signatureSecret, string payload, string @event);

        Task<TransfeeraResponseCreatePixImmediate> CashIn(string payerName, string payerDocument, decimal value);

        Task<long> GetBalance();

        Task<int> CashOut(string idempotencyKey, TransfeeraCreateTransfer transfer);
    }
}

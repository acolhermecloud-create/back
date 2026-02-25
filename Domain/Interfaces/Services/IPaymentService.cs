using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface IPaymentService
    {
        #region Transactions

        Task<TransactionData> GeneratePix(int value, 
            string clientIp, Domain.Objects.ReflowPay.Item? item, User user,
            string? webhook);

        Task ConfirmTransaction(Gateway gateway, string transactionId);

        Task<bool> ConfirmPix(Gateway gateway, string transactionId);

        #endregion
    }
}

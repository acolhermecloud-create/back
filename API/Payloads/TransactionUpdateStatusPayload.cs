using Domain;

namespace API.Payloads
{
    public class TransactionUpdateStatusPayload
    {
        public string TransactionId { get; set; }
        public BankTransactionStatus Status { get; set; }
    }
}

using Domain;

namespace API.Payloads
{
    public class GetTransactionsPayload
    {
        public List<BankTransactionStatus> Statuses { get; set; }
        public List<BankTransactionType> Types { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

namespace Domain.Objects
{
    public class TransactionData
    {
        public string Id { get; set; }
        public int Value { get; set; }
        public TransationMethod TransationMethod { get; set; }
        public string TransactionName { get; set; }
        public Gateway Gateway { get; set; }
        public string? QRCode { get; set; }
        public string? Code { get; set; }
        public BankTransactionSource TransactionSource { get; set; }
    }
}

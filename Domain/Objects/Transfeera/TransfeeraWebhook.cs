using Newtonsoft.Json;

namespace Domain.Objects.Transfeera
{
    public class TransfeeraWebhook
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class TransfeeraBank
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("ispb")]
        public object Ispb { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_description")]
        public object StatusDescription { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }

        [JsonProperty("integration_id")]
        public string IntegrationId { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("idempotency_key")]
        public string IdempotencyKey { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [JsonProperty("batch_id")]
        public string BatchId { get; set; }

        [JsonProperty("receipt_url")]
        public string ReceiptUrl { get; set; }

        [JsonProperty("bank_receipt_url")]
        public string BankReceiptUrl { get; set; }

        [JsonProperty("payment_date")]
        public object PaymentDate { get; set; }

        [JsonProperty("finish_date")]
        public DateTime FinishDate { get; set; }

        [JsonProperty("transfer_date")]
        public DateTime TransferDate { get; set; }

        [JsonProperty("returned_date")]
        public object ReturnedDate { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("pix_description")]
        public object PixDescription { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("pix_end2end_id")]
        public object PixEnd2endId { get; set; }

        [JsonProperty("emv")]
        public object Emv { get; set; }

        [JsonProperty("is_withdraw")]
        public bool IsWithdraw { get; set; }

        [JsonProperty("authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("partial")]
        public bool? Partial { get; set; }

        [JsonProperty("end2end_id")]
        public string? End2endId { get; set; }

        [JsonProperty("txid")]
        public string? TxId { get; set; }

        [JsonProperty("transfer_id")]
        public string? TransferId { get; set; }

        [JsonProperty("payer")]
        public TransfeeraPayer? Payer { get; set; }

        [JsonProperty("key_type")]
        public string KeyType { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("ownership_date")]
        public DateTime? OwnershipDate { get; set; }

        [JsonProperty("analysis_status", NullValueHandling = NullValueHandling.Ignore)]
        public string AnalysisStatus { get; set; }

        [JsonProperty("analysis_due_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime AnalysisDueDate { get; set; }

        [JsonProperty("analysis_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? AnalysisDate { get; set; }

        [JsonProperty("analysis_description", NullValueHandling = NullValueHandling.Ignore)]
        public string AnalysisDescription { get; set; }

        [JsonProperty("situation_type", NullValueHandling = NullValueHandling.Ignore)]
        public string SituationType { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public int Amount { get; set; }

        [JsonProperty("infraction_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime InfractionDate { get; set; }

        [JsonProperty("infraction_description", NullValueHandling = NullValueHandling.Ignore)]
        public string InfractionDescription { get; set; }

        [JsonProperty("payer_name", NullValueHandling = NullValueHandling.Ignore)]
        public string PayerName { get; set; }

        [JsonProperty("payer_tax_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PayerTaxId { get; set; }

        [JsonProperty("contested_at", NullValueHandling = NullValueHandling.Ignore)]
        public object ContestedAt { get; set; }

        [JsonProperty("refund", NullValueHandling = NullValueHandling.Ignore)]
        public Refund Refund { get; set; }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public TransfeeraUser User { get; set; }

        [JsonProperty("destination_bank_account")]
        public DestinationBankAccount DestinationBankAccount { get; set; }
    }

    public class DestinationBankAccount
    {
        public string PixKeyType { get; set; }

        public string PixKey { get; set; }
    }

    public class TransfeeraPayer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("document")]
        public string Document { get; set; }

        [JsonProperty("account_type")]
        public string? AccountType { get; set; }

        [JsonProperty("account")]
        public string? Account { get; set; }

        [JsonProperty("account_digit")]
        public string? AccountDigit { get; set; }

        [JsonProperty("agency")]
        public string? Agency { get; set; }

        [JsonProperty("bank")]
        public TransfeeraBank? Bank { get; set; }

        [JsonProperty("address")]
        public TransfeeraAddress? Address { get; set; }
    }

    public class DestinationBankAccountWebhook
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("cpf_cnpj")]
        public object CpfCnpj { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }

        [JsonProperty("pix_key")]
        public string PixKey { get; set; }

        [JsonProperty("pix_key_type")]
        public string PixKeyType { get; set; }

        [JsonProperty("status_description")]
        public object StatusDescription { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("bank")]
        public Bank Bank { get; set; }
    }

    public class Refund
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("analysis_status", NullValueHandling = NullValueHandling.Ignore)]
        public string AnalysisStatus { get; set; }

        [JsonProperty("transaction_id", NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionId { get; set; }

        [JsonProperty("refunded_amount", NullValueHandling = NullValueHandling.Ignore)]
        public int RefundedAmount { get; set; }

        [JsonProperty("refund_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime RefundDate { get; set; }

        [JsonProperty("rejection_reason", NullValueHandling = NullValueHandling.Ignore)]
        public object RejectionReason { get; set; }
    }

    public class Root
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }
    }

    public class TransfeeraUser
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}

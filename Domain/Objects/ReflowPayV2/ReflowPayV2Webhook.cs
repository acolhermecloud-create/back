using Newtonsoft.Json;

namespace Domain.Objects.ReflowPayV2
{
    public class ReflowPayV2Webhook
    {
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("endToEndId")]
        public string EndToEndId { get; set; }

        [JsonProperty("processedAt")]
        public DateTime ProcessedAt { get; set; }

        [JsonProperty("payer")]
        public Payer Payer { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("observation")]
        public object Observation { get; set; }
    }

    public class Payer
    {
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        [JsonProperty("bankName")]
        public object BankName { get; set; }

        [JsonProperty("bankCode")]
        public object BankCode { get; set; }

        [JsonProperty("bankIspb")]
        public string BankIspb { get; set; }
    }
}

using Newtonsoft.Json;

namespace Domain.Objects.Bloobank
{
    public class BloobankTransactionWebhook
    {
        [JsonProperty("topicId", NullValueHandling = NullValueHandling.Ignore)]
        public string TopicId { get; set; }

        [JsonProperty("messageId", NullValueHandling = NullValueHandling.Ignore)]
        public string MessageId { get; set; }

        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("pathname", NullValueHandling = NullValueHandling.Ignore)]
        public string Pathname { get; set; }

        [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
        public Body Body { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Params Params { get; set; }
    }

    public class Body
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("orgId", NullValueHandling = NullValueHandling.Ignore)]
        public string OrgId { get; set; }

        [JsonProperty("merchantId", NullValueHandling = NullValueHandling.Ignore)]
        public string MerchantId { get; set; }

        [JsonProperty("rcaId", NullValueHandling = NullValueHandling.Ignore)]
        public object RcaId { get; set; }

        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public object Fee { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public Amount Amount { get; set; }

        [JsonProperty("paidAmount", NullValueHandling = NullValueHandling.Ignore)]
        public object PaidAmount { get; set; }

        [JsonProperty("receivedAmount", NullValueHandling = NullValueHandling.Ignore)]
        public object ReceivedAmount { get; set; }

        [JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
        public Customer Customer { get; set; }

        [JsonProperty("pix", NullValueHandling = NullValueHandling.Ignore)]
        public Pix Pix { get; set; }

        [JsonProperty("billet", NullValueHandling = NullValueHandling.Ignore)]
        public object Billet { get; set; }

        [JsonProperty("creditCard", NullValueHandling = NullValueHandling.Ignore)]
        public object CreditCard { get; set; }

        [JsonProperty("installments", NullValueHandling = NullValueHandling.Ignore)]
        public int Installments { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata Metadata { get; set; }

        [JsonProperty("historic", NullValueHandling = NullValueHandling.Ignore)]
        public List<Historic> Historic { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("approvedAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ApprovedAt { get; set; }

        [JsonProperty("cancelledAt", NullValueHandling = NullValueHandling.Ignore)]
        public object CancelledAt { get; set; }
    }

    public class Historic
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("agent", NullValueHandling = NullValueHandling.Ignore)]
        public string Agent { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public object Message { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedAt { get; set; }
    }

    public class Params
    {
    }
}

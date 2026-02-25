using Newtonsoft.Json;

namespace Domain.Objects.Bloobank
{
    public class ResponseGeneratePayment
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orgId")]
        public string OrgId { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("rcaId")]
        public string RcaId { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public Amount Amount { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("pix")]
        public Pix Pix { get; set; }

        [JsonProperty("installments")]
        public int Installments { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("approvedAt")]
        public string ApprovedAt { get; set; }

        [JsonProperty("cancelledAt")]
        public string CancelledAt { get; set; }
    }

    public class Amount
    {
        [JsonProperty("ccy")]
        public string Ccy { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class Customer
    {
        [JsonProperty("doc")]
        public Doc Doc { get; set; }
    }

    public class Doc
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class Metadata
    {
    }

    public class Pix
    {
        [JsonProperty("qrcode")]
        public string Qrcode { get; set; }

        [JsonProperty("end2EndId")]
        public object End2EndId { get; set; }

        [JsonProperty("endToEndId")]
        public object EndToEndId { get; set; }

        [JsonProperty("copypaste")]
        public string Copypaste { get; set; }
    }
}

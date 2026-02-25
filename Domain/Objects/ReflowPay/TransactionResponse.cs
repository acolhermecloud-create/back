using Newtonsoft.Json;

namespace Domain.Objects.ReflowPay
{
    public class CustomerReflowPay
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

    public class Invoice
    {
        [JsonProperty("identification")]
        public string Identification { get; set; }

        [JsonProperty("digitableLine")]
        public string DigitableLine { get; set; }

        [JsonProperty("invoiceUrl")]
        public string InvoiceUrl { get; set; }
    }

    public class Pix
    {
        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("encodedImage")]
        public string EncodedImage { get; set; }
    }

    public class TransactionResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("referenceCode")]
        public string ReferenceCode { get; set; }

        [JsonProperty("externalCode")]
        public string ExternalCode { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("flag")]
        public string Flag { get; set; }

        [JsonProperty("expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty("pix")]
        public Pix Pix { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("customer")]
        public CustomerReflowPay Customer { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}

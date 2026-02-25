using System.Text.Json.Serialization;

namespace Domain.Objects.ReflowPay
{
    public class ReflowpayTransactionWebhook
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("externalCode")]
        public string? ExternalCode { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("storeId")]
        public string? StoreId { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}

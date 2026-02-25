using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Utm
    {
        public static readonly string TABLE_NAME = "utms";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid CampaignId { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; } = "Acolher";

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = "pix";

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("approvedDate")]
        public string? ApprovedDate { get; set; }

        [JsonPropertyName("refundedAt")]
        public string? RefundedAt { get; set; } = null;

        [JsonPropertyName("customer")]
        public UtmCustomer? Customer { get; set; }

        [JsonPropertyName("products")]
        public UtmProduct[]? Products { get; set; }

        [JsonPropertyName("trackingParameters")]
        public UtmTrackingParameters TrackingParameters { get; set; }

        [JsonPropertyName("commission")]
        public UtmCommission Commission { get; set; }

        [JsonPropertyName("isTest")]
        public bool IsTest { get; set; } = false;
    }

    public class UtmCustomer
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("document")]
        public string Document { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = "BR";

        [JsonPropertyName("ip")]
        public string? Ip { get; set; }
    }

    public class UtmProduct
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("planId")]
        public string PlanId { get; set; }

        [JsonPropertyName("planName")]
        public string PlanName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("priceInCents")]
        public int PriceInCents { get; set; }
    }

    public class UtmTrackingParameters
    {
        [JsonPropertyName("src")]
        public string? Src { get; set; }

        [JsonPropertyName("sck")]
        public string? Sck { get; set; }

        [JsonPropertyName("utm_source")]
        public string? UtmSource { get; set; }

        [JsonPropertyName("utm_campaign")]
        public string? UtmCampaign { get; set; }

        [JsonPropertyName("utm_medium")]
        public string? UtmMedium { get; set; }

        [JsonPropertyName("utm_content")]
        public string? UtmContent { get; set; }

        [JsonPropertyName("utm_term")]
        public string? UtmTerm { get; set; }

        [JsonPropertyName("utm_id")]
        public string? UtmId { get; set; }

        [JsonPropertyName("fbclid")]
        public string? Fbclid { get; set; }

        // ✅ Google Ads
        [JsonPropertyName("gclid")]
        public string? Gclid { get; set; }

        // Afiliado
        [JsonPropertyName("sub1")]
        public string? Sub1 { get; set; }
    }

    public class UtmCommission
    {
        [JsonPropertyName("totalPriceInCents")]
        public int TotalPriceInCents { get; set; }

        [JsonPropertyName("gatewayFeeInCents")]
        public int GatewayFeeInCents { get; set; } = 0;

        [JsonPropertyName("userCommissionInCents")]
        public int UserCommissionInCents { get; set; }
    }
}

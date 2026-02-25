using Newtonsoft.Json;

namespace Domain.Objects.ReflowPayV2
{
    public class ReflowV2CashinResponse
    {
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("txId")]
        public string TxId { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("qrCode")]
        public string QrCode { get; set; }

        [JsonProperty("qrCodeBase64")]
        public string QrCodeBase64 { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("gatewayId")]
        public int GatewayId { get; set; }
    }
}

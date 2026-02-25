using Newtonsoft.Json;

namespace Domain.Objects.Transfeera
{
    public class TransfeeraResponseCreatePixImmediate
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("qrcode_type")]
        public string QrcodeType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("txid")]
        public string Txid { get; set; }

        [JsonProperty("integration_id")]
        public string IntegrationId { get; set; }

        [JsonProperty("pix_key")]
        public PixKey PixKey { get; set; }

        [JsonProperty("expiration")]
        public int? Expiration { get; set; }

        [JsonProperty("original_value")]
        public decimal? OriginalValue { get; set; }

        [JsonProperty("value_change_mode")]
        public string ValueChangeMode { get; set; }

        [JsonProperty("payer_question")]
        public string PayerQuestion { get; set; }

        [JsonProperty("payer")]
        public Payer Payer { get; set; }

        [JsonProperty("reject_unknown_payer")]
        public bool? RejectUnknownPayer { get; set; }

        [JsonProperty("emv_payload")]
        public string EmvPayload { get; set; }

        [JsonProperty("image_base64")]
        public string ImageBase64 { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class Payer
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
        public Bank? Bank { get; set; }

        [JsonProperty("address")]
        public TransfeeraAddress? Address { get; set; }
    }

    public class Bank
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

    public class PixKey
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("key_type")]
        public string KeyType { get; set; }
    }

    public class TransfeeraAddress
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("complement")]
        public string Complement { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
    }
}

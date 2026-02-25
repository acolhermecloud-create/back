using Newtonsoft.Json;

namespace Domain.Objects.Transfeera
{
    public class TransfeeraResponseGetWebhooks
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("company_id")]
        public string CompanyId { get; set; }

        [JsonProperty("application_id")]
        public string ApplicationId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("object_types")]
        public List<string> ObjectTypes { get; set; }

        [JsonProperty("schema_version")]
        public string SchemaVersion { get; set; }

        [JsonProperty("signature_secret")]
        public string SignatureSecret { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public object DeletedAt { get; set; }
    }
}

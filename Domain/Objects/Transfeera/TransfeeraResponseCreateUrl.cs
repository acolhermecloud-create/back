using Newtonsoft.Json;

namespace Domain.Objects.Transfeera
{
    public class TransfeeraResponseCreateUrl
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("signature_secret")]
        public string SignatureSecret { get; set; }
    }
}

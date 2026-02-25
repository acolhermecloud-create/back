using Newtonsoft.Json;

namespace Domain.Objects.Venit
{
    public class ResponseConsultPixKey
    {
        [JsonProperty("Success")]
        public string Success { get; set; }

        [JsonProperty("Chave")]
        public string Chave { get; set; }

        [JsonProperty("Beneficiario")]
        public Beneficiario Beneficiario { get; set; }

        [JsonProperty("ENDTOEND")]
        public string ENDTOEND { get; set; }
    }

    public class Beneficiario
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("branch")]
        public string Branch { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("participant")]
        public Participant Participant { get; set; }

        [JsonProperty("holder")]
        public Holder Holder { get; set; }
    }

    public class Holder
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("document")]
        public string Document { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Participant
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ispb")]
        public string Ispb { get; set; }
    }
}

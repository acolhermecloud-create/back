using Newtonsoft.Json;

namespace Domain.Objects.Venit
{
    public class ResponseTransferMoney
    {
        [JsonProperty("Success")]
        public string Success { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}

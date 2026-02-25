using Newtonsoft.Json;

namespace Domain.Objects.Transfeera
{
    public class TransfeeraResponseGetBalance
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("waiting_value")]
        public decimal WaitingValue { get; set; }
    }
}

using Newtonsoft.Json;

namespace Domain.Objects.Venit
{
    public class ResponseGetSaldo
    {
        [JsonProperty("Success")]
        public string Success { get; set; }

        [JsonProperty("Saldo Disponível")]
        public string SaldoDisponvel { get; set; }

        [JsonProperty("Saldo Bloqueado")]
        public string SaldoBloqueado { get; set; }
    }
}

using Domain.Interfaces.Services;
using Domain.Objects.Venit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace Service
{
    public class VenitService : IVenitService
    {
        private readonly string _apiBaseUrl = "https://conta.venitbank.com.br/api";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _username;
        private readonly string _password;

        private readonly HttpClient _httpClient;

        public VenitService(IConfiguration configuration, HttpClient httpClient)
        {
            _clientId = configuration["Venit:ClientId"];
            _clientSecret = configuration["Venit:ClientSecret"];
            _username = configuration["Venit:Username"];
            _password = configuration["Venit:Password"];

            _httpClient = httpClient;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("client-id", _clientId);
            request.Headers.Add("client-secret", _clientSecret);

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}")));

            request.Headers.Add("Idempotency", Guid.NewGuid().ToString());

            return request;
        }


        public async Task<long> GetBalance()
        {
            var request = CreateRequest(HttpMethod.Post, $"{_apiBaseUrl}/GetSaldo");
            request.Headers.Add("Idempotency", Guid.NewGuid().ToString());

            var contents = new StringContent(string.Empty);
            contents.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = contents;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return 0;

            var content = await response.Content.ReadAsStringAsync();
            var saldo = JsonConvert.DeserializeObject<ResponseGetSaldo>(content);

            if (saldo == null || string.IsNullOrWhiteSpace(saldo.SaldoDisponvel)) return 0;

            string saldoFormatado = saldo.SaldoDisponvel.Replace("R$", "").Trim();
            long balanceF = (long)(decimal.Parse(saldoFormatado, NumberStyles.Currency, new CultureInfo("pt-BR")) * 100);
            return balanceF;
        }

        public async Task<ResponseConsultPixKey?> ConsultPixKeyAsync(string pixKey)
        {
            var request = CreateRequest(HttpMethod.Post, $"{_apiBaseUrl}/GetPixKey");

            var contents = new StringContent(string.Empty);
            contents.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = new StringContent(JsonConvert.SerializeObject(new { chavePix = pixKey }), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<ResponseConsultPixKey>(await response.Content.ReadAsStringAsync())
                : null;
        }

        public async Task<ResponseTransferMoney> SendPixAsync(ResponseConsultPixKey responseConsultPixKey, decimal value)
        {
            var contentJson = new
            {
                valor = value,
                responseConsultPixKey.Chave,
                descricao = "TRANSFERÊNCIA KAIXINHA",
                responseConsultPixKey.ENDTOEND,
                Beneficiario = new
                {
                    number = responseConsultPixKey.Beneficiario.Number,
                    branch = responseConsultPixKey.Beneficiario.Branch,
                    type = responseConsultPixKey.Beneficiario.Type,
                    participantIspb = responseConsultPixKey.Beneficiario.Participant.Ispb,
                    holder = new
                    {
                        document = responseConsultPixKey.Beneficiario.Holder.Document,
                        name = responseConsultPixKey.Beneficiario.Holder.Name,
                        type = responseConsultPixKey.Beneficiario.Holder.Type
                    }
                }
            };

            var request = CreateRequest(HttpMethod.Post, $"{_apiBaseUrl}/EnviarPixChave");

            var contents = new StringContent(string.Empty);
            contents.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = new StringContent(JsonConvert.SerializeObject(contentJson), Encoding.UTF8, 
                new MediaTypeHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ResponseTransferMoney>(content);
        }
    }
}

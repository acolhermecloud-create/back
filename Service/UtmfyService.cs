using Domain;
using Domain.Interfaces.Services;
using iugu.net.Response;
using Microsoft.Extensions.Configuration;
using Sentry.Protocol;
using System.Text;
using System.Text.Json;

namespace Service
{
    public class UtmfyService(HttpClient httpClient,
        IConfiguration configuration): IUtmfyService
    {
        private readonly HttpClient _httpClient = httpClient;

        private readonly string _apiUrl = "https://api.utmify.com.br/api-credentials/orders";
        private readonly string _apiToken = configuration["UtmfyToken"]!;

        public async Task<string> SendEvent(Utm utm)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiToken))
                    throw new Exception("API token não definido.");

                var json = JsonSerializer.Serialize(
                    utm,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                Console.WriteLine($"📤 UTMIFY SEND | OrderId: {utm.OrderId}");
                Console.WriteLine($"📦 PAYLOAD: {json}");

                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("x-api-token", _apiToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📥 UTMIFY STATUS: {(int)response.StatusCode}");
                Console.WriteLine($"📥 UTMIFY RESPONSE: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    var responseMsg = $"Erro Utmify | Status: {(int)response.StatusCode} | {responseContent}";
                    Console.WriteLine($"❌ {responseMsg}");
                    SentrySdk.CaptureException(new Exception(responseMsg));

                    return responseMsg;
                }

                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
                var errorMessage = $"Erro Utmify | Status: 400 | {ex.Message}";
                return errorMessage;
            }
        }
    }
}

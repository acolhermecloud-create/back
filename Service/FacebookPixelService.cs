using Domain;
using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Service
{
    public class FacebookPixelService(HttpClient httpClient,
        IConfiguration configuration) : IFacebookPixelService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> SendEventToFacebookAsync(string pixelId, string accessToken, string eventName, string transactionId, Utm eventParams)
        {
            var apiUrl = $"https://graph.facebook.com/v21.0/{pixelId}/events?access_token={accessToken}";

            if (eventParams == null) return false;

            var testEventCode = _configuration["Facebook:PixelTest"];

            var requestBody = new Dictionary<string, object>
            {
                ["data"] = new[]
                {
                    new
                    {
                        event_name = eventName,
                        event_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        event_id = HashData(transactionId),
                        action_source = "website",

                        user_data = new
                        {
                            em = !string.IsNullOrEmpty(eventParams?.Customer?.Email)
                                ? HashData(eventParams.Customer.Email)
                                : null,

                            ph = !string.IsNullOrEmpty(eventParams?.Customer?.Phone)
                                ? HashData(eventParams.Customer.Phone)
                                : null,

                            fbc = GenerateFbc(eventParams?.TrackingParameters?.Fbclid),

                            client_ip_address = eventParams.Customer.Ip
                        },

                        custom_data = new
                        {
                            value = Math.Round(eventParams.Products.First().PriceInCents / 100.0, 2),
                            currency = "BRL",
                            order_id = eventParams.OrderId,

                            utm_source = eventParams.TrackingParameters.UtmSource,
                            utm_medium = eventParams.TrackingParameters.UtmMedium,
                            utm_campaign = eventParams.TrackingParameters.UtmCampaign,
                            utm_term = eventParams.TrackingParameters.UtmTerm,
                            utm_content = eventParams.TrackingParameters.UtmContent
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(testEventCode))
            {
                requestBody["test_event_code"] = testEventCode;
            }

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Erro: {response.StatusCode} - {responseString}");
                    return false;
                }

                Console.WriteLine($"Sucesso: {responseString}");
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao enviar evento: {ex.Message}");
                return false;
            }
        }

        private string HashData(string data)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        private string GenerateFbp()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = RandomNumberGenerator.GetInt32(100000000, int.MaxValue);
            return $"fb.1.{timestamp}.{random}";
        }

        private string? GenerateFbc(string? fbclid)
        {
            if (string.IsNullOrEmpty(fbclid))
                return null;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"fb.1.{timestamp}.{fbclid}";
        }

    }
}

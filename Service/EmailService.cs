using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Service
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private const string URLBASE = "https://api.brevo.com";

        private readonly IConfiguration _configuration = configuration;

        public async Task Send(string[] to, string subject, string body, string businessAddress, string businessName,
            Dictionary<string, string>? base64Attachments)
        {
            var url = $"{URLBASE}/v3/smtp/email";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("api-key", _configuration["Mail:ApiKey"]);

            var requestData = new
            {
                sender = new
                {
                    name = businessName,
                    email = businessAddress
                },
                to = to.Select(email => new { email }).ToArray(),
                subject,
                htmlContent = body
            };

            var jsonRequest = JsonConvert.SerializeObject(requestData);
            var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
            }
            else
                throw new Exception(responseContent);
        }
    }
}

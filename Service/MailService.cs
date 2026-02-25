using Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using Newtonsoft.Json;

namespace Service
{
    public class MailService : IMailService
    {
        private const string URLBASE = "https://api.brevo.com";
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var url = $"{URLBASE}/v3/smtp/email";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("api-key", _configuration["Mail:ApiKey"]);

            var requestData = new
            {
                sender = new
                {
                    name = _configuration["Mail:SenderName"],
                    email = _configuration["Mail:SenderEmail"]
                },
                to = new[]
                {
                    new{ email = to }
                },
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

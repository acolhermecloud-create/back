using Domain.Acquirers;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects.Transfeera;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Service
{
    public class TransfeeraService
        (ITransfeeraRepository transfeeraRepository,
        IConfiguration configuration) : ITransfeeraService
    {
        private readonly ITransfeeraRepository _transfeeraRepository = transfeeraRepository;
        private readonly IConfiguration _configuration = configuration;

        public async Task<TransfeeraResponseCreatePixImmediate> CashIn(string payerName, string payerDocument, decimal value)
        {
            using var client = new HttpClient();
            try
            {
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var pixKey = $"{_configuration["Transfeera:PixKey"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                var url = $"{_configuration["Transfeera:PaymentUrl"]}/pix/qrcode/collection/immediate";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName} ({corporateEmail})");

                var jsonContent = JsonConvert.SerializeObject(new
                {
                    payer = new
                    {
                        name = payerName,
                        document = payerDocument,
                    },
                    integration_id = Guid.NewGuid().ToString(),
                    value_change_mode = "VALOR_FIXADO",
                    pix_key = pixKey,
                    original_value = value
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var pixImmediateResponse = JsonConvert.DeserializeObject<TransfeeraResponseCreatePixImmediate>(responseContent);

                    return pixImmediateResponse;
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um código de erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error creating transaction batch: {ex.Message}");
            }
        }

        public async Task<int> CashOut(string idempotencyKey, TransfeeraCreateTransfer tranfer)
        {
            int batchId = await CreateTransactionBatch();
            int transactionId = await CreateTransaction(batchId, idempotencyKey, tranfer);
            await ProcessTransactionBatch(batchId);

            return transactionId;
        }

        public async Task<long> GetBalance()
        {
            using var client = new HttpClient();
            try
            {
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var pixKey = $"{_configuration["Transfeera:PixKey"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                var url = $"{_configuration["Transfeera:PaymentUrl"]}/statement/balance";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName} ({corporateEmail})");

                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseGetBalance = JsonConvert.DeserializeObject<TransfeeraResponseGetBalance>(responseContent);

                    return (long)(responseGetBalance.Value * 100);
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um código de erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error creating transaction batch: {ex.Message}");
            }
            throw new NotImplementedException();
        }

        public async Task SetTransfeeraWebhook(string[] events, string webhookUrl)
        {
            using var client = new HttpClient();
            try
            {
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                // URL de consulta para verificar webhooks existentes
                var getUrl = $"{_configuration["Transfeera:PaymentUrl"]}/webhook";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName} ({corporateEmail})");

                // Consultar as URLs de webhook existentes
                var consultaResponse = await client.GetAsync(getUrl);
                var consultaResponseContent = await consultaResponse.Content.ReadAsStringAsync();

                if (consultaResponse.IsSuccessStatusCode)
                {
                    var webhooks = JsonConvert.DeserializeObject<List<TransfeeraResponseGetWebhooks>>(consultaResponseContent);

                    // Verificar se algum webhook contém os eventos passados e excluí-los
                    foreach (var webhook in webhooks)
                    {
                        if (webhook.ObjectTypes.Intersect(events).Any())  // Verifica se algum evento do webhook coincide com os eventos passados
                        {
                            var removeUrl = $"{_configuration["Transfeera:PaymentUrl"]}/webhook/{webhook.Id}";
                            var removeResponse = await client.DeleteAsync(removeUrl);

                            if (!removeResponse.IsSuccessStatusCode)
                            {
                                throw new Exception($"Failed to remove existing webhook: {removeResponse.StatusCode}");
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception($"API error while checking webhooks: {consultaResponse.StatusCode} - {consultaResponseContent}");
                }

                // Agora adicionar a nova URL de webhook
                var addUrl = $"{_configuration["Transfeera:PaymentUrl"]}/webhook";
                var requestBody = new
                {
                    url = $"{webhookUrl}",
                    object_types = events  // Usando os eventos passados para criar o novo webhook
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(addUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseCreateUrl = JsonConvert.DeserializeObject<TransfeeraResponseCreateUrl>(responseContent);

                    // TENHO QUE SALVAR O SECRET
                    Transfeera transfeera = await _transfeeraRepository.Get();

                    if (events.Contains("CashIn"))
                    {
                        transfeera ??= new();
                        transfeera.WebhookCashInSecret = responseCreateUrl.SignatureSecret;
                        await _transfeeraRepository.AddOrUpdate(transfeera);
                    }
                    else if (events.Contains("Transfer"))
                    {
                        transfeera ??= new();
                        transfeera.WebhookTransferSecret = responseCreateUrl.SignatureSecret;
                        await _transfeeraRepository.AddOrUpdate(transfeera);
                    }
                }
                else
                {
                    throw new Exception($"API error while setting webhook: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error pointing to webhook: {ex.Message}");
            }
        }

        public async Task<bool> ValidateSignature(string signatureSecret, string payload, string @event)
        {
            var webhooksecret = await _transfeeraRepository.Get();

            string transfeeraSignatureHeader = signatureSecret;

            // Extraindo o timestamp (t) e a assinatura (v1) do header
            var signatureParts = transfeeraSignatureHeader.Split(',');
            var timestamp = signatureParts[0].Split('=')[1]; // '1580306991086'
            var signature = signatureParts[1].Split('=')[1]; // '348a92ec7864e30fc9cf3ea91b2e6e1392a14c8379103cb1d8e48e39334a4fd8'

            // Chave secreta usada na criação do webhook
            string secretKey = @event == "CashIn" ? webhooksecret.WebhookCashInSecret : webhooksecret.WebhookTransferSecret;

            // Preparando a string para assinatura
            string signedPayload = $"{timestamp}.{payload}";

            // Log para verificar o payload e a string que estamos assinando
            Console.WriteLine($"Payload: {payload}");
            Console.WriteLine($"Signed Payload: {signedPayload}");

            // Calculando a assinatura usando HMAC com SHA-256
            string calculatedSignature = CalculateHMACSHA256(signedPayload, secretKey);

            // Log para verificar a assinatura calculada
            Console.WriteLine($"Calculated Signature: {calculatedSignature}");

            // Comparando a assinatura calculada com a assinatura recebida no header
            bool isSignatureValid = calculatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);

            return isSignatureValid;
        }

        protected async Task<bool> ProcessTransactionBatch(int batchId)
        {
            using var client = new HttpClient();
            try
            {
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var pixKey = $"{_configuration["Transfeera:PixKey"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                var url = $"{_configuration["Transfeera:PaymentUrl"]}/batch/{batchId}/close";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName} ({corporateEmail})");

                // Não há um corpo na requisição (a requisição parece ser uma simples solicitação para fechar o batch)
                var content = new StringContent(""); // Corpo vazio, caso seja necessário

                // Realizar a requisição POST
                var response = await client.PostAsync(url, content);

                // Verificar se a resposta foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Caso de sucesso, retorna o código da resposta
                    return response.IsSuccessStatusCode;
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um código de erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error processing batch of transactions: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error making request to process batch: {ex.Message}");
            }
        }

        protected async Task<int> CreateTransaction(int bathId, string idempotencyKey,
            TransfeeraCreateTransfer transfer, string type = "TRANSFERENCIA")
        {
            using var client = new HttpClient();
            try
            {
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var pixKey = $"{_configuration["Transfeera:PixKey"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                var url = $"{_configuration["Transfeera:PaymentUrl"]}/batch/{bathId}/transfer";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName}  ({corporateEmail})");

                var requestBody = new
                {
                    value = transfer.Value,
                    integration_id = idempotencyKey,
                    idempotency_key = idempotencyKey,
                    destination_bank_account = new
                    {
                        pix_key_type = transfer.DestinationBankAccount.PixKeyType,
                        pix_key = transfer.DestinationBankAccount.PixKey
                    },
                    pix_key_validation = new
                    {
                        cpf_cnpj = transfer.PixKeyValidation.CpfCnpj
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var batchResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    return (int)batchResponse.id;
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um código de erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error creating transaction batch: {ex.Message}");
            }
        }

        protected async Task<int> CreateTransactionBatch(string type = "TRANSFERENCIA")
        {
            using var client = new HttpClient();
            try
            {
                // Obter o token de autenticação
                var clientId = $"{_configuration["Transfeera:ClientId"]}";
                var clientSecret = $"{_configuration["Transfeera:ClientSecret"]}";
                var pixKey = $"{_configuration["Transfeera:PixKey"]}";
                var corporateName = $"{_configuration["Transfeera:CorporateName"]}";
                var corporateEmail = $"{_configuration["Transfeera:CorporateEmail"]}";

                var token = await GetTokenTransfeera("client_credentials", clientId, clientSecret, corporateName, corporateEmail);

                var url = $"{_configuration["Transfeera:PaymentUrl"]}/batch";

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName}  ({corporateEmail})");

                var jsonContent = JsonConvert.SerializeObject(new { type });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var batchResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    return (int)batchResponse.id;
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um código de erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com erros de requisição ou outros problemas
                throw new Exception($"Error creating transaction batch: {ex.Message}");
            }
        }

        private async Task<string> GetTokenTransfeera(string grantType, string clientId, string clientSecret, string corporateName, string email)
        {
            using var client = new HttpClient();
            try
            {
                var url = $"{_configuration["Transfeera:AuthUrl"]}/authorization";

                // Definir o cabeçalho User-Agent com o formato correto
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd($"{corporateName} ({email})");

                // Criar o corpo da requisição com os dados necessários
                var requestBody = new
                {
                    grant_type = grantType,
                    client_id = clientId,
                    client_secret = clientSecret
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                // Verificar se a resposta foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    // Ler a resposta em formato JSON
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Aqui, você pode processar a resposta e extrair o token, caso a resposta seja válida
                    var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return tokenResponse.access_token; // Supondo que o token esteja em "access_token"
                }
                else
                {
                    // Caso de falha, lançar uma exceção ou retornar um erro
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error getting token: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Lidar com qualquer erro que ocorrer na requisição
                throw new Exception($"Error making the request to obtain the token: {ex.Message}");
            }
        }

        protected static string CalculateHMACSHA256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder hexString = new(2 * hash.Length);
            foreach (byte b in hash)
            {
                string hex = b.ToString("x2");
                hexString.Append(hex);
            }
            return hexString.ToString();
        }
    }
}

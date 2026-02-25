using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;
using Domain.Objects.Bloobank;
using Domain.Objects.ReflowPayV2;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IGatewayConfigurationRepository _gatewayConfigurationRepository;
        private readonly ITransfeeraService _transfeeraService;

        private SemaphoreSlim _semaphore; // Limita a 50 threads simultâneas

        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly IUtilityService _utilityService;

        public PaymentService(
        ICacheService cacheService,
        IGatewayConfigurationRepository gatewayConfigurationRepository,
        IConfiguration configuration,
        IUtilityService utilityService,
        ITransfeeraService transfeeraService)
        {
            _configuration = configuration;
            _cacheService = cacheService;
            _utilityService = utilityService;

            _gatewayConfigurationRepository = gatewayConfigurationRepository;

            // Obtém o número máximo de threads da configuração e converte para inteiro
            var maxThreads = int.Parse(configuration["System:PaymentMaxNumberOfThreads"] ?? "100");  // Default para 50 caso não haja configuração
            _semaphore = new SemaphoreSlim(maxThreads);  // Inicializa o SemaphoreSlim com o número configurado
            _transfeeraService = transfeeraService;
        }

        public async Task<TransactionData> GeneratePix(int value, string clientIp,
        Domain.Objects.ReflowPay.Item? item, User user,
        string? webhook)
        {
            var gateway = await _gatewayConfigurationRepository.Get();
            TransactionData transactionData = null;

            // Tenta gerar o pagamento dentro da limitação de threads
            async Task<TransactionData?> TryGenerate(Gateway gatewayOption)
            {
                await _semaphore.WaitAsync();  // Espera até que uma thread esteja disponível

                try
                {
                    var paymentMethod = "pix";

                    if (gatewayOption == Gateway.ReflowPay)
                    {
                        var transactionId = Guid.NewGuid();
                        var installments = 1;

                        Domain.Objects.ReflowPay.Customer customer = new()
                        {
                            Name = user.Name,
                            Email = user.Email,
                            Phone = !string.IsNullOrEmpty(user.Phone) ? user.Phone : "5511930751522",
                            Document = new Domain.Objects.ReflowPay.Document() { Number = user.DocumentId, Type = user.DocumentId.Length == 11 ? "cpf" : "cnpj" }
                        };

                        var transaction = await GenerateTransactionReflowPay(transactionId.ToString(),
                            paymentMethod, installments, clientIp, [item], customer);

                        if (transaction.Pix == null || transaction.Pix.EncodedImage == null || transaction.Pix.Payload == null)
                            return null;

                        return new TransactionData
                        {
                            Id = transaction.Id,
                            TransactionName = paymentMethod,
                            TransationMethod = TransationMethod.Cash,
                            Gateway = Gateway.ReflowPay,
                            QRCode = transaction.Pix.EncodedImage,
                            Code = transaction.Pix.Payload,
                            Value = value
                        };
                    }
                    if (gatewayOption == Gateway.ReflowPayV2)
                    {
                        var transactionId = Guid.NewGuid();
                        var installments = 1;

                        var transaction = await GenerateTransactionReflowPayV2(user.Name,
                            user.DocumentId,
                            user.Email,
                            !string.IsNullOrEmpty(user.Phone) ? user.Phone : "5511930751522",
                            item.Title,
                            item.Description,
                            value);

                        if (!transaction.Success || string.IsNullOrEmpty(transaction.QrCodeBase64))
                            return null;

                        return new TransactionData
                        {
                            Id = transaction.TransactionId.ToString(),
                            TransactionName = paymentMethod,
                            TransationMethod = TransationMethod.Cash,
                            Gateway = Gateway.ReflowPayV2,
                            QRCode = transaction.QrCodeBase64,
                            Code = transaction.QrCode,
                            Value = value
                        };
                    }
                    else if (gatewayOption == Gateway.Bloobank)
                    {
                        var transaction = await GenerateTransactionBloobank(user, paymentMethod, value);

                        return new TransactionData
                        {
                            Id = transaction.Id,
                            TransactionName = paymentMethod,
                            TransationMethod = TransationMethod.Cash,
                            Gateway = Gateway.Bloobank,
                            QRCode = await _utilityService.DownloadImageAsBase64(transaction.Pix.Qrcode),
                            Code = transaction.Pix.Copypaste,
                            Value = value
                        };
                    }
                    else if (gatewayOption == Gateway.Transfeera)
                    {
                        var valueInDecimal = _utilityService.ConvertCentsToDecimal(value);

                        var transaction = await _transfeeraService.CashIn(user.Name, user.DocumentId, valueInDecimal);

                        var base64Prefix = "data:image/png;base64,";
                        var qrCode = transaction.ImageBase64.StartsWith(base64Prefix)
                            ? transaction.ImageBase64
                            : base64Prefix + transaction.ImageBase64;

                        return new TransactionData
                        {
                            Id = transaction.IntegrationId,
                            TransactionName = paymentMethod,
                            TransationMethod = TransationMethod.Cash,
                            Gateway = Gateway.Transfeera,
                            QRCode = qrCode,
                            Code = transaction.EmvPayload,
                            Value = value
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao gerar pagamento no {gatewayOption}: {ex.Message}");
                    return null; // Retorna null em caso de erro
                }
                finally
                {
                    _semaphore.Release();  // Libera a thread para a próxima execução
                }

                // Retorno de fallback, para garantir que sempre haverá um valor
                return null;
            }

            transactionData = await TryGenerate(gateway.Pix);

            if (transactionData == null && gateway.TryToGenerateCashInInOtherAcquirers)
            {
                foreach (var fallbackGateway in Enum.GetValues<Gateway>())
                {
                    if (fallbackGateway == gateway.Pix)
                        continue;

                    transactionData = await TryGenerate(fallbackGateway);

                    if (transactionData != null)
                        break;
                }
            }

            if (transactionData == null)
                throw new Exception("Não foi possível processar o pagamento, tente novamente mais tarde.");

            var key = $"{transactionData.Gateway}_{transactionData.Id}";
            await _cacheService.Set(key, JsonConvert.SerializeObject(false), 24);

            return transactionData;
        }

        public async Task<bool> ConfirmPix(Gateway gateway, string transactionId)
        {
            var key = $"{gateway}_{transactionId}";

            var cache = await _cacheService.Get(key);
            if (cache == null) return false;

            bool value = JsonConvert.DeserializeObject<bool>(cache);
            if (!value) return false;

            return true;
        }

        public async Task ConfirmTransaction(Gateway gateway, string transactionId)
        {
            var key = $"{gateway}_{transactionId}";

            var cache = await _cacheService.Get(key);
            if (cache == null) return;

            await _cacheService.Set(key, JsonConvert.SerializeObject(true), 72);
        }

        protected async Task<ReflowV2CashinResponse> GenerateTransactionReflowPayV2(
            string payerName,
            string payerDocument,
            string payerEmail,
            string payerPhone,
            string productName,
            string productDescription,
            long value
            )
        {
            // Objeto para armazenar a transação
            var transactionPayload = new
            {
                payerName,
                payerDocument,
                payerEmail,
                payerPhone,
                productName,
                productDescription,
                value,
                orderId = Guid.NewGuid().ToString(),
                postbackUrl = $"{_configuration["System:BackendUrl"]}/webhook/payment/confirm"
            };

            using var httpClient = new HttpClient();

            var jsonPayload = JsonConvert.SerializeObject(transactionPayload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://cashin.safepayments.cloud/transaction/qrcode/cashin"),
                Headers =
                {
                    { "Authorization", $"Bearer {_configuration["ReflowPayV2ApiKey"]}" },
                },
                Content = content
            };
            var response = await httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"REFLOW_V2 RESPONSE: {body}");
            response.EnsureSuccessStatusCode();

            var transactionResult = JsonConvert.DeserializeObject<ReflowV2CashinResponse>(body);
            // Retorna o resultado
            return transactionResult;
        }

        protected async Task<Domain.Objects.ReflowPay.TransactionResponse> GenerateTransactionReflowPay(
            string externalTransactionId,
            string paymentMethod,
            int installments,
            string clientIP,
            Domain.Objects.ReflowPay.Item[] items,
            Domain.Objects.ReflowPay.Customer customer,
            Domain.Objects.ReflowPay.Card? creditCard = null
            )
        {
            // Objeto para armazenar a transação
            var transactionPayload = new
            {
                isInfoProducts = true,
                externalCode = externalTransactionId,
                discount = 0.0,
                paymentMethod, // Ajustar conforme método de pagamento
                installments, // Ajustar para o número correto de parcelas
                customer,
                items,
                postbackUrl = $"{_configuration["System:BackendUrl"]}/webhook/payment/confirm",
                ip = clientIP,

            };

            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(3)
            };

            var jsonPayload = JsonConvert.SerializeObject(transactionPayload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.reflowpay.com/v1/transactions"),
                Headers =
                {
                    { "x-authorization-key", _configuration["ReflowPayApiKey"] },
                },
                Content = content
            };
            var response = await httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"REFLOW RESPONSE: {body}");
            response.EnsureSuccessStatusCode();

            var transactionResult = JsonConvert.DeserializeObject<Domain.Objects.ReflowPay.TransactionResponse>(body);
            // Retorna o resultado
            return transactionResult;
        }

        protected async Task<ResponseGeneratePayment> GenerateTransactionBloobank(User user, string method, int amount)
        {
            var dict = new Dictionary<string, object>
            {
                { "method", method },
                { "amount", new { ccy = "BRL", value = amount } },
                { "customer", new {
                    doc = new { type = user.DocumentId.Length == 11 ? "CPF" : "CNPJ", value = user.DocumentId },
                    name = user.Name
                } },
                { "installments", 1 },
            };

            var body = JsonConvert.SerializeObject(dict);

            var (accessKey, timestamp, signature) = GenerateSignature(body);

            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(3)
            };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://payment.blooapis.io/v1/payments"),
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Access-Key", accessKey);
            request.Headers.Add("X-Access-Timestamp", timestamp);
            request.Headers.Add("X-Access-Signature", signature);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ResponseGeneratePayment>(responseBody);
            else
                throw new Exception(responseBody);
        }

        private (string accessKey, string timestamp, string signature) GenerateSignature(string body)
        {
            var accessKey = _configuration["Bloobank:AccessKey"];
            var privateKey = _configuration["Bloobank:PrivateKey"].Replace(":","");

            // Timestamp em milissegundos (Unix Time).
            var timestamp = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() -1000).ToString();
            Console.WriteLine("Timestamp: " + timestamp);

            // Monta a mensagem para hashing e assinatura:
            // msg = accessKey + '|' + body + '|' + timestamp
            var msg = $"{accessKey}|{body}|{timestamp}";

            // Calcula o hash SHA-256 da mensagem
            byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
            byte[] msgHash = SHA256.HashData(msgBytes);

            // Assina a mensagem (o hash) com ECDSA secp256k1
            var signature = SignSecp256k1(privateKey, msgHash);

            return (accessKey, timestamp, signature);
        }

        private static string SignSecp256k1(string privateKey, byte[] hash)
        {
            // Converte a chave privada de hex para bytes
            var privateKeyBytes = Convert.FromHexString(privateKey);

            // Carrega parâmetros da curva secp256k1
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            // Cria objeto de chave privada
            var d = new BigInteger(1, privateKeyBytes);
            var privateKeyParams = new ECPrivateKeyParameters(d, domainParams);

            // Cria o assinador. Usamos NONEwithECDSA, pois já calculamos o hash fora.
            ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(true, privateKeyParams);

            // "Alimenta" o hash para o assinador e gera a assinatura em formato DER
            signer.BlockUpdate(hash, 0, hash.Length);
            var signatureDer = signer.GenerateSignature();

            // Converte para Base64
            return Convert.ToBase64String(signatureDer);
        }
    }
}

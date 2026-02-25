using Domain;
using Domain.Bank;
using Domain.Interfaces.Repository.Bank;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.Objects;
using Domain.Objects.Aggregations;
using Domain.Objects.Transfeera;
using Domain.System;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Service
{
    public class BankService(IBankAccountRepository bankAccountRepository,
        IBankTransactionRepository bankTransactionRepository,
        IConfiguration configuration,
        ITransfeeraService transfeeraService,
        IEmailService mailService,
        IBaasConfigurationRepository baasConfigurationRepository,
        ICacheService cacheService) : IBankService
    {
        private readonly IBankAccountRepository _bankAccountRepository = bankAccountRepository;
        private readonly IBankTransactionRepository _bankTransactionRepository = bankTransactionRepository;
        private readonly IBaasConfigurationRepository _baasConfigurationRepository = baasConfigurationRepository;

        private readonly IConfiguration _configuration = configuration;
        private readonly ITransfeeraService _transfeeraService = transfeeraService;
        private readonly IEmailService _mailService = mailService;
        private readonly ICacheService _cacheService = cacheService;

        public async Task MakeSplitToSystemAccount(Guid userId, Guid donationId, decimal amountToBeDivided, 
            BankSplitType bankSplitType, decimal amountSplit, decimal fixedRate)
        {
            var systemBankAccount = await _bankAccountRepository.GetSystemAccount();
            var userBankAccount = await _bankAccountRepository.GetByUserId(userId);

            if(userBankAccount == null)
            {
                BankAccount account = new(userId, "", 0, BankAccountType.Customer);
                await _bankAccountRepository.CreateAsync(account);

                userBankAccount = account;
            }

            BankTransaction transactionSystem = null;
            BankTransaction transactionCustomer = null;
            decimal totalSystem, totalToUser;

            if (bankSplitType == BankSplitType.Percent)
                totalSystem = (amountToBeDivided * amountSplit) / 100;
            else
                totalSystem = amountSplit;

            decimal totalFees = fixedRate + totalSystem;
            totalToUser = amountToBeDivided - totalFees;

            // SPLIT PARA SISTEMA
            transactionSystem = new(systemBankAccount.Id, donationId, 0, 0, totalFees, 
                BankTransactionType.CashIn, BankTransactionStatus.Completed, BankTransactionSource.Campaign, $"Split {userId}", null, DateTime.Now);

            // SPLIT PARA USUÁRIO
            transactionCustomer = new(userBankAccount.Id, donationId, amountToBeDivided, totalFees, totalToUser, 
                BankTransactionType.CashIn, BankTransactionStatus.WaitingForRelease, BankTransactionSource.Campaign, $"Recebimento {donationId}", null, null);

            await _bankTransactionRepository.Create(transactionSystem);
            await _bankTransactionRepository.Create(transactionCustomer);

            userBankAccount.Balance += totalToUser;
            systemBankAccount.Balance += totalSystem;

            await _bankAccountRepository.Update(userBankAccount);
            await _bankAccountRepository.Update(systemBankAccount);
        }

        public async Task<bool> RequestWithDraw(Guid userId, long value, Guid? transactionId = null)
        {
            try
            {
                var account = await _bankAccountRepository.GetByUserId(userId);
                if (account == null) throw new InvalidOperationException("Cadastre uma chave PIX para solicitar saque");

                long balanceInCents = await GetBalance(userId);

                if (value > balanceInCents)
                    throw new InvalidOperationException("Saldo insuficiente");

                var baasconfig = await _baasConfigurationRepository.Get();
                if (baasconfig != null && value < baasconfig.DailyWithdrawalMinimumValue)
                    throw new InvalidOperationException($"Saque mínimo é de R$ {baasconfig.DailyWithdrawalMinimumValue / 100m:F2}");

                if (baasconfig != null)
                {
                    var cashout = await _bankTransactionRepository.GetDailyBankTransactions(
                        account.Id, BankTransactionStatus.Completed, BankTransactionType.CashOut,
                        DateTime.Now
                    );

                    long totalWithdrawnToday = cashout.Sum(t => (long)(t.Liquid * 100)); // Converte para centavos

                    if ((totalWithdrawnToday + value) > baasconfig.DailyWithdrawalLimitValue)
                        throw new InvalidOperationException($"Você ultrapassou o limite diário de saque de R$ {baasconfig.DailyWithdrawalLimitValue / 100m:F2}");
                }

                long totalDonationsValue = (long)(Math.Truncate(account.Balance * 10000m) / 100);
                decimal withDraw = value / 100m;

                var balanceAvaliable = await _transfeeraService.GetBalance();

                // Se não existir transactionId, cria uma nova transação
                if (!transactionId.HasValue)
                {
                    var transaction = CreateTransaction(account, withDraw, baasconfig);
                    await _bankTransactionRepository.Create(transaction);
                    transactionId = transaction.Id;

                    string subject = $"Solicitação de saque de R$ {withDraw}";
                    decimal systemBalance = balanceAvaliable / 100m;
                    string body = $@"
                        <html>
                            <body>
                                <h2>Detalhes da solicitação</h2>
                                <p><strong>Valor:</strong> R$ {withDraw}</p>
                                <p><strong>Saldo em caixa:</strong> {systemBalance}</p>
                            </body>
                        </html>
                    ";
                    _ = SendMailNotification(subject, body);  // Dispara a notificação por email
                }

                string jobId;

                if (value > balanceAvaliable)
                {
                    // Reagendar nova tentativa sem recriar a transação
                    jobId = BackgroundJob.Schedule(() => RequestWithDraw(userId, value, transactionId), TimeSpan.FromMinutes(60));
                }
                else
                {
                    // Se a configuração exigir análise, retorna true
                    if (baasconfig?.AnalyseWithdraw == true)
                    {
                        return true;
                    }

                    // Agendar saque para execução em 1 hr
                    jobId = BackgroundJob.Schedule(() => WithDraw(transactionId.ToString(), account, withDraw), TimeSpan.FromSeconds(1));
                }
                await _cacheService.Set(transactionId.ToString(), jobId, 1);

                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message);  // Lançar a exceção de forma mais explícita
            }
            catch (Exception ex)
            {
                // Para erros genéricos, capturamos a exceção
                throw new Exception("Erro inesperado ao processar o saque", ex);
            }
        }

        public async Task MakeWithDraw(Guid transactionId, BankTransactionStatus transactionStatus)
        {
            var transaction = await _bankTransactionRepository.GetById(transactionId);

            if (transaction == null)
                throw new Exception("Transação não existe");

            if(transactionStatus == BankTransactionStatus.Canceled) // AQUI ELE CANCELA A TRANSFERENCIA
            {
                transaction.ReasonToFailed = "Saque cancelado";
                transaction.Status = transactionStatus;
                transaction.ProcessedAt = DateTime.Now;
                await _bankTransactionRepository.Update(transaction);

                var jobInCache = await _cacheService.Get(transactionId.ToString());
                if(jobInCache != null)
                {
                    try
                    {
                        BackgroundJob.Delete(jobInCache);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                } // REMOVE O PROCESSO EM BACKGROUND DE SAQUE
            }
            else if(transactionStatus == BankTransactionStatus.Completed) // AQUI ELE TENTA FAZER A TRANSFERENCIA
            {
                var balanceAvaliable = await _transfeeraService.GetBalance();
                if (transaction.Liquid > balanceAvaliable)
                    throw new Exception("Não há saldo suficiente no BAAS");

                var account = await _bankAccountRepository.GetById(transaction.AccountId);

                if (account == null || account.PixKey == null)
                    throw new Exception("Chave PIX do beneficiário não foi informada");

                TransfeeraCreateTransfer transfer = new()
                {
                    Value = transaction.Liquid,
                    DestinationBankAccount = new()
                    {
                        PixKey = account.PixKey,
                        PixKeyType = account.PixKey.Length == 11 ? "CPF" : "CNPJ"
                    },
                    PixKeyValidation = new()
                    {
                        CpfCnpj = account.PixKey
                    }
                };

                var withdraw = await _transfeeraService.CashOut(transaction.Id.ToString(), transfer);

                await _cacheService.Set($"{withdraw}", JsonConvert.SerializeObject(transaction), 1488);
            }
        }

        public async Task UpdateTransactionStatus(Guid transactionId, BankTransactionStatus transactionStatus)
        {
            var transaction = await _bankTransactionRepository.GetById(transactionId);

            if (transaction == null)
                throw new Exception("Transação não existe");

            transaction.Status = transactionStatus;
            transaction.ProcessedAt = DateTime.Now;
            await _bankTransactionRepository.Update(transaction);
        }

        public async Task WithDraw(string transactionId, BankAccount bankAccount, decimal value) // Processar saque
        {
            var parseTransactionId = Guid.Parse(transactionId);

            var transaction = await _bankTransactionRepository.GetById(parseTransactionId);

            if (transaction.Status != BankTransactionStatus.Pending) return;

            if (bankAccount == null || string.IsNullOrEmpty(bankAccount.PixKey))
            {
                transaction.ReasonToFailed = "Chave PIX do beneficiário está em branco";
                transaction.Status = BankTransactionStatus.Failed;
                transaction.ProcessedAt = DateTime.Now;
                await _bankTransactionRepository.Update(transaction);
                return;
            }

            TransfeeraCreateTransfer transfer = new()
            {
                Value = transaction.Liquid,
                DestinationBankAccount = new()
                {
                    PixKey = bankAccount.PixKey,
                    PixKeyType = bankAccount.PixKey.Length == 11 ? "CPF" : "CNPJ"
                },
                PixKeyValidation = new()
                {
                    CpfCnpj = bankAccount.PixKey
                }
            };

            var withdraw = await _transfeeraService.CashOut(transaction.Id.ToString(), transfer);
            await _cacheService.Set($"{withdraw}", JsonConvert.SerializeObject(transaction), int.MaxValue);
        }

        public async Task<BankAccount> GetSystemAccount()
        {
            return await _bankAccountRepository.GetSystemAccount();
        }

        public async Task AddTransaction(BankTransaction bankTransaction)
        {
            await _bankTransactionRepository.Create(bankTransaction);
        }

        public async Task ReleaseBalance() // LIBERA A DINHEIRO DEPOIS DE X DIAS
        {
            var transactions = await _bankTransactionRepository.GetBankTransactionsWaitingRelease();

            var daysToReleaseStr = _configuration["System:DaysToRelease"];
            var daysToRelease = int.Parse(daysToReleaseStr ?? "7");

            var twoDaysAgo = DateTime.Now.AddDays(-daysToRelease);
            var transactionsPreparedToRelease = transactions.Where(x => x.CreatedAt <= twoDaysAgo).ToList();

            if (transactionsPreparedToRelease.Count == 0)
                return; // Não faz nada se não houver dinheiro para liberar

            foreach (var transaction in transactionsPreparedToRelease)
            {
                transaction.Status = BankTransactionStatus.Completed;
                transaction.ProcessedAt = DateTime.Now;
                await _bankTransactionRepository.Update(transaction);
            }
        }

        public async Task<PagedResult<BankTransaction>> ListCashOutTransactions(Guid userId, int page, int pageSize)
        {
            var account = await _bankAccountRepository.GetByUserId(userId);
            if (account == null) return new PagedResult<BankTransaction>();

            return await _bankTransactionRepository.GetTransactionByType(account.Id, BankTransactionType.CashOut, page, pageSize);
        }

        public async Task<PagedResult<BankTransactionWithAccountDto>> ListTransactions(
            List<BankTransactionStatus> transactionStatuses,
            List<BankTransactionType> transactionTypes, int page, int pageSize)
        {
            var systemAccount = await _bankAccountRepository.GetSystemAccount();

            var transactions = 
                await _bankTransactionRepository.GetTransactionByType(transactionStatuses, transactionTypes, page, pageSize,
                systemAccount.Id);

            return transactions;
        }

        public async Task CreateAccount(Guid userId, BankAccountType bankAccountType, string? pixKey)
        {
            var accountrepo = await _bankAccountRepository.GetByUserId(userId);
            if (accountrepo != null)
            {
                await _bankAccountRepository.UpdatePixKey(accountrepo.Id, pixKey);
                return;
            }

            BankAccount account = new(userId, pixKey, 0, bankAccountType);
            await _bankAccountRepository.CreateAsync(account);
        }

        public async Task<BalanceExtractResume> GetWaitingRelease(Guid userId)
        {
            var account = await _bankAccountRepository.GetByUserId(userId);
            if (account == null) return new BalanceExtractResume();

            var transactions = await _bankTransactionRepository
                .GetBankTransactions(account.Id, BankTransactionStatus.WaitingForRelease, BankTransactionType.CashIn);

            return new BalanceExtractResume
            {
                GrossValue = transactions.Sum(t => (long)(t.Gross * 100)),
                Tax = transactions.Sum(t => (long)(t.Tax * 100)),
                Liquid = transactions.Sum(t => (long)(t.Liquid * 100))
            };
        }

        public async Task<BalanceExtractResume> GetReleased(Guid userId)
        {
            var account = await _bankAccountRepository.GetByUserId(userId);
            if (account == null) return new BalanceExtractResume();

            var transactions = await _bankTransactionRepository
                .GetBankTransactions(account.Id, BankTransactionStatus.Completed, BankTransactionType.CashIn);

            return new BalanceExtractResume
            {
                GrossValue = transactions.Sum(t => (long)(t.Gross * 100)),
                Tax = transactions.Sum(t => (long)(t.Tax * 100)),
                Liquid = transactions.Sum(t => (long)(t.Liquid * 100))
            };
        }

        public async Task<long> GetBalance(Guid userId)
        {
            var account = await _bankAccountRepository.GetByUserId(userId);
            if (account == null) return 0;

            var cashout = await _bankTransactionRepository.GetTransactionByType(account.Id, BankTransactionType.CashOut, new() { BankTransactionStatus.Completed, BankTransactionStatus.Pending, BankTransactionStatus.AwaitingApproval });
            var cashin = await _bankTransactionRepository.GetTransactionByType(account.Id, BankTransactionType.CashIn, new() { BankTransactionStatus.Completed });

            long balance = cashin.Sum(t => (long)(t.Liquid * 100)) - cashout.Sum(t => (long)(t.Liquid * 100));

            return balance;
        }

        public async Task<BaasConfiguration> GetBAASConfiguration()
        {
            return await _baasConfigurationRepository.Get();
        }

        public async Task SetBAASConfiguration(bool analyseWithdraw, long dailyWithdrawalLimitValue, long dailyWithdrawalMinimumValue)
        {
            var baasConfiguration = await _baasConfigurationRepository.Get();
            baasConfiguration ??= new();
            baasConfiguration.AnalyseWithdraw = analyseWithdraw;
            baasConfiguration.DailyWithdrawalLimitValue = dailyWithdrawalLimitValue;
            baasConfiguration.DailyWithdrawalMinimumValue = dailyWithdrawalMinimumValue;

            await _baasConfigurationRepository.Add(baasConfiguration);
        }

        protected async Task SendMailNotification(string subject, string body)
        {
            string[] receivers = _configuration["Mail:WebMasters"].Split(",");
            string title = subject;
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            await _mailService.Send(receivers, title, body, businessAddresses,
                businessName, null);
        }

        private BankTransaction CreateTransaction(BankAccount account, decimal withDraw, BaasConfiguration baasconfig)
        {
            if (baasconfig?.AnalyseWithdraw == true)
            {
                return new BankTransaction(
                    account.Id,
                    null,
                    0,
                    0,
                    withDraw,
                    BankTransactionType.CashOut,
                    BankTransactionStatus.AwaitingApproval,
                    BankTransactionSource.Campaign,
                    $"Saque no valor de R$ {withDraw}",
                    null,
                    null
                );
            }
            else
            {
                return new BankTransaction(
                    account.Id,
                    null,
                    0,
                    0,
                    withDraw,
                    BankTransactionType.CashOut,
                    BankTransactionStatus.Pending,
                    BankTransactionSource.Campaign,
                    $"Saque no valor de R$ {withDraw}",
                    null,
                    null
                );
            }
        }

        public async Task RefundTransaction(Guid transactionId)
        {
            var bankTransaction = await _bankTransactionRepository.GetById(transactionId);
            if (bankTransaction == null) return;

            bankTransaction.Status = BankTransactionStatus.Refund;
            bankTransaction.Description = "Reembolsado";
            bankTransaction.ProcessedAt = DateTime.Now;

            await _bankTransactionRepository.Update(bankTransaction);
        }
    }
}

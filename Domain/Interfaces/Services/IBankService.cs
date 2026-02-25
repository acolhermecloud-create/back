using Domain.Bank;
using Domain.Objects;
using Domain.Objects.Aggregations;
using Domain.System;

namespace Domain.Interfaces.Services
{
    public interface IBankService
    {
        Task CreateAccount(Guid userId, BankAccountType bankAccountType, string? pixKey);

        Task AddTransaction(BankTransaction bankTransaction);

        Task MakeSplitToSystemAccount(Guid userId, Guid donationId, 
            decimal amountToBeDivided, BankSplitType bankSplitType, decimal amountSplit, decimal fixedRate);

        Task<bool> RequestWithDraw(Guid userId, long value, Guid? transactionId = null);

        Task MakeWithDraw(Guid transactionId, BankTransactionStatus transactionStatus);

        Task UpdateTransactionStatus(Guid transactionId, BankTransactionStatus transactionStatus);

        Task<PagedResult<BankTransaction>> ListCashOutTransactions(Guid userId, int page, int pageSize);

        Task<PagedResult<BankTransactionWithAccountDto>> ListTransactions(List<BankTransactionStatus> transactionStatuses,
            List<BankTransactionType> transactionTypes, int page, int pageSize);

        Task<BalanceExtractResume> GetWaitingRelease(Guid userId);
        Task<BalanceExtractResume> GetReleased(Guid userId);
        Task<long> GetBalance(Guid userId);
        Task<BankAccount> GetSystemAccount();

        Task<BaasConfiguration> GetBAASConfiguration();

        Task SetBAASConfiguration(bool analyseWithdraw, long dailyWithdrawalLimitValue, long dailyWithdrawalMinimumValue);

        Task RefundTransaction(Guid transactionId);

        //Jobs
        Task ReleaseBalance();
    }
}

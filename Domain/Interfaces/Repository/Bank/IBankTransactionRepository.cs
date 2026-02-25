using Domain.Bank;
using Domain.Objects;
using Domain.Objects.Aggregations;

namespace Domain.Interfaces.Repository.Bank
{
    public interface IBankTransactionRepository
    {
        Task<BankTransaction?> GetById(Guid id);

        Task<PagedResult<BankTransaction>> GetTransactionByType(Guid userId, BankTransactionType transactionType, int page, int pageSize);

        Task<PagedResult<BankTransactionWithAccountDto>> GetTransactionByType(
            List<BankTransactionStatus> transactionStatuses,
            List<BankTransactionType> transactionTypes, int page, int pageSize,
            Guid? accountIdToExclude = null);

        Task<List<BankTransaction>> GetTransactionByType(Guid accountId, BankTransactionType transactionType, List<BankTransactionStatus> transactionStatuses);

        Task Create(BankTransaction transaction);

        Task Update(BankTransaction transaction);

        Task<List<BankTransaction>> GetBankTransactionsWaitingRelease();

        Task<List<BankTransaction>> GetBankTransactions(Guid accountId, BankTransactionStatus status, BankTransactionType bankTransactionType);

        Task<List<BankTransaction>> GetDailyBankTransactions(Guid accountId, BankTransactionStatus status, BankTransactionType bankTransactionType,
            DateTime date);

        // Custom querys
        Task<List<BankTransaction>> GetAllTransactionsExceptTheSystemOne(Guid systemAccountId, DateTime startDate, DateTime endDate, BankTransactionType type);
        Task<List<BankTransaction>> GetTransactions(DateTime startDate, DateTime endDate, BankTransactionType type);
        Task<List<BankTransaction>> GetTransactions(DateTime startDate, DateTime endDate, BankTransactionType type, BankTransactionSource bankTransactionSource);
    }
}

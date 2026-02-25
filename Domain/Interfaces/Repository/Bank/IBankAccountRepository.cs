using Domain.Bank;

namespace Domain.Interfaces.Repository.Bank
{
    public interface IBankAccountRepository
    {
        Task<BankAccount?> GetById(Guid id);
        Task<BankAccount> GetByUserId(Guid userId);
        Task CreateAsync(BankAccount account);
        Task Update(BankAccount account);
        Task UpdatePixKey(Guid accountId, string pixKey);
        Task DeleteAsync(Guid id);

        Task<BankAccount> GetSystemAccount(); // obtem a conta bancária do sistema
    }
}

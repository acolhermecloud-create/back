namespace Domain.Interfaces.Repository
{
    public interface IUserPaymentAccountRepository
    {
        Task Add(UserPaymentAccount userPaymentAccount);
        Task<List<UserPaymentAccount>> GetByUserId(Guid userId);
        Task Update(UserPaymentAccount userPaymentAccount);
        Task<UserPaymentAccount?> GetByAccountId(string accountId);
        Task DeactivateAccount(Guid accountId);
    }
}

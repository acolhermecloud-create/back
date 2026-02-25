namespace Domain.Interfaces.Repository
{
    public interface IUserDigitalStickersRepository
    {
        Task Add(UserDigitalStickers userDigitalStickers);
        Task Update(UserDigitalStickers userDigitalStickers);
        Task<List<UserDigitalStickers>> GetByUserId(Guid userId);
        Task<UserDigitalStickers> GetByTransactionId(string transactionId);
        Task<UserDigitalStickers> GetByIdAsync(Guid id);
    }
}

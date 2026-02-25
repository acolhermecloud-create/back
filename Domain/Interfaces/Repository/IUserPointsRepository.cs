namespace Domain.Interfaces.Repository
{
    public interface IUserPointsRepository
    {
        Task Add(UserPoints userPoints);
        Task Update(UserPoints userPoints);
        Task<List<UserPoints>> GetMoreThan(int page, int pageSize, int points);
        Task<UserPoints> GetByUserId(Guid userId);
    }
}

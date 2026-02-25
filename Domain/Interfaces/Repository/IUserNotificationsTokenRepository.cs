namespace Domain.Interfaces.Repository
{
    public interface IUserNotificationsTokenRepository
    {
        Task Add(UserNotificationsTokens userNotification);
        Task<UserNotificationsTokens> GetByUserId(Guid userId);
        Task<List<UserNotificationsTokens>> GetAll();
        Task<List<UserNotificationsTokens>> GetByIds(List<Guid> ids);
    }
}

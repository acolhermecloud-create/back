namespace Domain.Interfaces.Repository
{
    public interface IUserNotificationsRepository
    {
        Task Add(UserNotifications notification);
        Task<List<UserNotifications>> GetByUserId(Guid userId);
        Task<UserNotifications> GetById(Guid id);
        Task Update(UserNotifications notification);
    }
}

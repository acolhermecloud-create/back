namespace Domain.Interfaces.Services
{
    public interface INotificationsService
    {
        Task RecordUserToken(Guid userId, string token);

        Task SendToAll(string title, string body, string? data);

        Task SendToSomeUsers(List<Guid> usersId, string title, string body, string? data);

        Task MarkAsRead(Guid userNotificationId);
    }
}

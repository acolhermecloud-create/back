using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Service
{
    public class NotificationsService : INotificationsService
    {
        private readonly IUserNotificationsTokenRepository _userNotificationsTokenRepository;
        private readonly IUserNotificationsRepository _userNotificationsRepository;

        public NotificationsService(IUserNotificationsTokenRepository userNotificationsTokenRepository,
            IUserNotificationsRepository userNotificationsRepository)
        {
            _userNotificationsTokenRepository = userNotificationsTokenRepository;
            _userNotificationsRepository = userNotificationsRepository;

            string? envEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string googleFCMCredentials = string.Empty;

            if (envEnvironment == "Production")
                googleFCMCredentials = $"Firebase{Path.DirectorySeparatorChar}prod-kaixinha-firebase.json";
            else
                googleFCMCredentials = $"Firebase{Path.DirectorySeparatorChar}dev-kaixinha-firebase.json";

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(googleFCMCredentials)
                });
            }
        }

        public async Task MarkAsRead(Guid userNotificationId)
        {
            var notification = await _userNotificationsRepository.GetById(userNotificationId);
            if (notification != null)
            {
                notification.Read = true;
                notification.UpdatedAt = DateTime.Now;
                await _userNotificationsRepository.Update(notification);
            }
        }

        public async Task RecordUserToken(Guid userId, string token)
        {
            var userToken = new UserNotificationsTokens(userId, token);
            await _userNotificationsTokenRepository.Add(userToken);
        }

        public async Task SendToAll(string title, string body, string? data)
        {
            var tokens = await _userNotificationsTokenRepository.GetAll();

            if (tokens == null || tokens.Count == 0)
                return;

            var message = new MulticastMessage()
            {
                Tokens = tokens.Select(x => x.Token).ToList(),  // Lista de tokens para os quais as notificações serão enviadas
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "extra_data", data ?? string.Empty }
                }
            };
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }

        public async Task SendToSomeUsers(List<Guid> usersId, string title, string body, string? data)
        {
            var tokens = await _userNotificationsTokenRepository.GetByIds(usersId);

            if (tokens == null || tokens.Count == 0)
                return;

            var message = new MulticastMessage()
            {
                Tokens = tokens.Select(x => x.Token).ToList(),  // Lista de tokens para os quais as notificações serão enviadas
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "extra_data", data ?? string.Empty }
                }
            };
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }
    }
}

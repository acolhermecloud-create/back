using Domain;
using Domain.Interfaces.Repository;
using Moq;
using Service;

namespace UnitTest.ServiceTest
{
    [TestFixture]
    public class NotificationsServiceTests
    {
        private Mock<IUserNotificationsTokenRepository> _mockUserNotificationsTokenRepository;
        private Mock<IUserNotificationsRepository> _mockUserNotificationsRepository;
        private NotificationsService _notificationsService;

        [SetUp]
        public void Setup()
        {
            _mockUserNotificationsTokenRepository = new Mock<IUserNotificationsTokenRepository>();
            _mockUserNotificationsRepository = new Mock<IUserNotificationsRepository>();

            _notificationsService = new NotificationsService(
                _mockUserNotificationsTokenRepository.Object,
                _mockUserNotificationsRepository.Object
            );
        }

        [Test]
        public async Task SendToAll_ShouldSendNotificationToAllTokens()
        {
            // Arrange
            var tokens = new List<UserNotificationsTokens>
        {
            new UserNotificationsTokens(Guid.NewGuid(), "token1"),
            new UserNotificationsTokens(Guid.NewGuid(), "token2")
        };

            _mockUserNotificationsTokenRepository
                .Setup(repo => repo.GetAll())
                .ReturnsAsync(tokens);

            // Act
            await _notificationsService.SendToAll("Test Title", "Test Body", "Test Data");

            // Assert
            _mockUserNotificationsTokenRepository.Verify(repo => repo.GetAll(), Times.Once);
            // Aqui podemos verificar se as notificações foram enviadas, mas como o Firebase Messaging é externo, 
            // isso pode ser feito com logs ou mocks mais avançados.
        }

        [Test]
        public async Task SendToSomeUsers_ShouldSendNotificationToSpecificUsers()
        {
            // Arrange
            var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var tokens = new List<UserNotificationsTokens>
        {
            new UserNotificationsTokens(userIds[0], "token1"),
            new UserNotificationsTokens(userIds[1], "token2")
        };

            _mockUserNotificationsTokenRepository
                .Setup(repo => repo.GetByIds(userIds))
                .ReturnsAsync(tokens);

            // Act
            await _notificationsService.SendToSomeUsers(userIds, "Test Title", "Test Body", "Test Data");

            // Assert
            _mockUserNotificationsTokenRepository.Verify(repo => repo.GetByIds(userIds), Times.Once);
        }

        [Test]
        public async Task RecordUserToken_ShouldStoreUserToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "userToken";

            // Act
            await _notificationsService.RecordUserToken(userId, token);

            // Assert
            _mockUserNotificationsTokenRepository.Verify(repo => repo.Add(It.Is<UserNotificationsTokens>(t => t.UserId == userId && t.Token == token)), Times.Once);
        }

        [Test]
        public async Task MarkAsRead_ShouldMarkNotificationAsRead()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var existingNotification = new UserNotifications(notificationId, "Title", "Body", "Data", false);
            _mockUserNotificationsRepository
                .Setup(repo => repo.GetById(notificationId))
                .ReturnsAsync(existingNotification);

            // Act
            await _notificationsService.MarkAsRead(notificationId);

            // Assert
            _mockUserNotificationsRepository.Verify(repo => repo.Update(It.Is<UserNotifications>(n => n.Read == true && n.UpdatedAt != null)), Times.Once);
        }
    }

}

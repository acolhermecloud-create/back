using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.System;
using Moq;
using Service;
using System.Security.Claims;

namespace UnitTest.ServiceTest
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<IUtilityService> _utilityServiceMock;
        private Mock<IMailService> _mailServiceMock;
        private Mock<ISocialAuthProvider> _socialAuthProviderMock;
        private Mock<IGroupAccessRepository> _groupAccessRepositoryMock;
        private Mock<IUserGroupAccessRepository> _userGroupAccessRepositoryMock;
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
        private Mock<IS3Service> _s3ServiceMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            // Inicializa os mocks das dependências
            _userServiceMock = new Mock<IUserService>();
            _utilityServiceMock = new Mock<IUtilityService>();
            _mailServiceMock = new Mock<IMailService>();
            _socialAuthProviderMock = new Mock<ISocialAuthProvider>();
            _groupAccessRepositoryMock = new Mock<IGroupAccessRepository>();
            _userGroupAccessRepositoryMock = new Mock<IUserGroupAccessRepository>();
            _s3ServiceMock = new Mock<IS3Service>();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            // Cria a instância do AuthService com dependências mocadas
            _authService = new AuthService(
                _userServiceMock.Object,
                _utilityServiceMock.Object,
                _socialAuthProviderMock.Object,
                _mailServiceMock.Object,
                _configurationMock.Object,
                _groupAccessRepositoryMock.Object,
                _userGroupAccessRepositoryMock.Object,
                _s3ServiceMock.Object);
        }

        //[Test]
        public async Task AuthenticateWithSocialMedia_ShouldReturnExistingUser_WhenUserAlreadyExists()
        {
            // Arrange
            var email = "existinguser@example.com";
            var token = "valid_social_token";
            var claims = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, "Existing User")
            }).Claims;

            _socialAuthProviderMock.Setup(provider => provider.ValidateToken("google", token)).ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(claims)));
            var existingUser = new User("Existing User", email, "", "", AuthProvider.Google, UserType.Common);
            _userServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync(existingUser);

            // Act
            var result = await _authService.AuthenticateWithSocialMedia("google", token);

            // Assert
            Assert.AreEqual(existingUser, result);
            _userServiceMock.Verify(service => service.GetByEmail(email), Times.Once);
            _userServiceMock.Verify(service => service.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserType>()), Times.Never);
        }

        //[Test]
        public async Task AuthenticateWithSocialMedia_ShouldCreateNewUser_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "newuser@example.com";
            var token = "valid_social_token";
            var claims = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, "New User")
            }).Claims;

            _socialAuthProviderMock.Setup(provider => provider.ValidateToken("google", token)).ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(claims)));
            _userServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync((User)null);

            // Act
            var result = await _authService.AuthenticateWithSocialMedia("google", token);

            // Assert
            _userServiceMock.Verify(service => service.Create(email, "New User", "", "", UserType.Common), Times.Once);
        }

        [Test]
        public async Task AuthenticateUserWithPassword_ShouldReturnToken_WhenEmailAndPasswordAreCorrect()
        {
            // Arrange
            var email = "user@example.com";
            var password = "Password123!";
            var hashedPassword = "hashed_password";
            var user = new User("User", email, hashedPassword, "", AuthProvider.None, UserType.Common);
            var groupAccessList = new List<GroupAccess>
            {
                new GroupAccess("Admin", "Acesso total ao sistema")
            };
            var userGroupAccessList = new List<UserGroupAccess> { new UserGroupAccess(user.Id, Guid.NewGuid()) };

            _userServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync(user);
            _utilityServiceMock.Setup(service => service.CryptSHA256(password)).Returns(hashedPassword);
            _userGroupAccessRepositoryMock.Setup(repo => repo.GetByUserId(user.Id)).ReturnsAsync(userGroupAccessList);
            _groupAccessRepositoryMock.Setup(repo => repo.GetById(It.IsAny<Guid>())).ReturnsAsync(groupAccessList[0]);
            _utilityServiceMock.Setup(service => service.GenerateToken(user, groupAccessList)).Returns("generated_token");

            // Act
            dynamic result = await _authService.AuthenticateUserWithPassword(email, password);

            // Assert
            Assert.IsNotNull(result, "O objeto retornado não possui a propriedade 'token'.");
        }

        [Test]
        public void AuthenticateUserWithPassword_ShouldThrowException_WhenEmailOrPasswordAreIncorrect()
        {
            // Arrange
            var email = "user@example.com";
            var password = "WrongPassword";
            _userServiceMock.Setup(service => service.GetByEmail(email)).ReturnsAsync((User)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _authService.AuthenticateUserWithPassword(email, password));
            Assert.That(ex.Message, Is.EqualTo("Email ou senha inválidos."));
        }
    }
}

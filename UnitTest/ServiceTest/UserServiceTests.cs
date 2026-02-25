using Amazon.Runtime.Internal.Transform;
using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.Bank;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.Objects;
using Domain.System;
using Moq;
using Service;

namespace UnitTest.ServiceTest
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IUserPointsRepository> _userPointsRepositoryMock;
        private Mock<IOngRepository> _ongRepositoryMock;
        private Mock<IAddressRepository> _addressRepositoryMock;
        private Mock<IUserGroupAccessRepository> _userGroupAccessRepositoryMock;
        private Mock<IGroupAccessRepository> _groupAccessRepositoryMock;
        private Mock<ICategoryRepository> _categoryRepositoryMock;
        private Mock<IPlanRepository> _planRepositoryMock;
        private Mock<IUtilityService> _utilityServiceMock;
        private Mock<IS3Service> _s3ServiceMock;
        private Mock<ICodeChallengeRepository> _codeChallengeMock;
        private Mock<IEmailService> _emailServiceMock;

        private Mock<IGatewayConfigurationRepository> _gatewayConfigurationMock;
        private Mock<IDigitalStickerRepository> _digitalStickerMock;
        private Mock<IBankAccountRepository> _bankAccountMock;
        private Mock<ICampaignRepository> _campaignMock;

        private MigrationService _migrationService;
        private UserService _userService;

        [SetUp]
        public async Task Setup()
        {
            // Inicializa os mocks das interfaces
            _userRepositoryMock = new Mock<IUserRepository>();
            _userPointsRepositoryMock = new Mock<IUserPointsRepository>();
            _ongRepositoryMock = new Mock<IOngRepository>();
            _userGroupAccessRepositoryMock = new Mock<IUserGroupAccessRepository>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _groupAccessRepositoryMock = new Mock<IGroupAccessRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _planRepositoryMock = new Mock<IPlanRepository>();
            _s3ServiceMock = new Mock<IS3Service>();
            _utilityServiceMock = new Mock<IUtilityService>();
            _codeChallengeMock = new Mock<ICodeChallengeRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _campaignMock = new Mock<ICampaignRepository>();

            _emailServiceMock = new Mock<IEmailService>();
            _gatewayConfigurationMock = new Mock<IGatewayConfigurationRepository>();
            _digitalStickerMock = new Mock<IDigitalStickerRepository>();
            _bankAccountMock = new Mock<IBankAccountRepository>();

            /*
             IGatewayConfigurationRepository gatewayConfigurationRepository,
        IDigitalStickerRepository digitalStickerRepository,
        IBankAccountRepository bankAccountRepository,
        ICampaignRepository campaignRepository
             */

            // Cria uma instância do MigrationService e executa a migração
            _migrationService = new MigrationService(_groupAccessRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _planRepositoryMock.Object,
                _gatewayConfigurationMock.Object,
                _digitalStickerMock.Object,
                _bankAccountMock.Object,
                _campaignMock.Object);

            // Configura o mock para retornar os grupos
            _groupAccessRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(new List<GroupAccess>
            {
                new GroupAccess("Admin", "Acesso total ao sistema"),
                new GroupAccess("User", "Acesso padrão ao sistema"),
                new GroupAccess("Guest", "Acesso restrito")
            });

            // Cria uma instância da UserService com as dependências mocadas
            _userService = new UserService(
                _userRepositoryMock.Object,
                _ongRepositoryMock.Object,
                _userGroupAccessRepositoryMock.Object,
                _groupAccessRepositoryMock.Object,
                _utilityServiceMock.Object,
                new Mock<Microsoft.Extensions.Configuration.IConfiguration>().Object,
                _s3ServiceMock.Object,
                _addressRepositoryMock.Object,
                _userPointsRepositoryMock.Object,
                _codeChallengeMock.Object,
                _emailServiceMock.Object);
        }

        /*
         IGroupAccessRepository groupAccessRepository,
        ICategoryRepository categoryRepository,
        IPlanRepository planRepository,
        IGatewayConfigurationRepository gatewayConfigurationRepository,
        IDigitalStickerRepository digitalStickerRepository,
        IBankAccountRepository bankAccountRepository,
        ICampaignRepository campaignRepository
         */

        [Test]
        public async Task IncrementPointsToUser_ShouldAddPoints()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var amount = 10;

            var userPoints = new UserPoints(userId, 0, new List<AddedPoints>());
            _userPointsRepositoryMock.Setup(repo => repo.GetByUserId(userId)).ReturnsAsync(userPoints);
            _userPointsRepositoryMock.Setup(repo => repo.Update(It.IsAny<UserPoints>())).Returns(Task.CompletedTask);

            // Act
            await _userService.IncrementPointsToUser(userId, amount);

            // Assert
            _userPointsRepositoryMock.Verify(repo => repo.Update(It.Is<UserPoints>(up => up.CurrentPoints == amount)), Times.Once);
        }
        [Test]
        public async Task GetUsersWhoHavePointsGreaterThan_ShouldReturnUsersWithPointsGreaterThanAmount()
        {
            // Arrange
            
            var user1 = new User("Test User 1", "test1@example.com", "hashedPassword", "documentId", AuthProvider.None, UserType.Common);
            var user2 = new User("Test User 2", "test2@example.com", "hashedPassword", "documentId", AuthProvider.None, UserType.Common);

            var userPoints1 = new UserPoints(user1.Id, 100, new List<AddedPoints>());
            var userPoints2 = new UserPoints(user2.Id, 150, new List<AddedPoints>());
            var amount = 50;
            var page = 1;
            var pageSize = 50;

            var usersWithPoints = new List<UserPoints> { userPoints1, userPoints2 };

            _userPointsRepositoryMock.Setup(repo => repo.GetMoreThan(amount, page, pageSize)).ReturnsAsync(usersWithPoints);

            // Simula o retorno de usuários baseado nos UserIds da lista de pontos
            _userRepositoryMock.Setup(repo => repo.GetById(user1.Id)).ReturnsAsync(user1);
            _userRepositoryMock.Setup(repo => repo.GetById(user2.Id)).ReturnsAsync(user2);

            // Act
            var result = await _userService.GetUsersWhoHavePointsGreaterThan(amount, page, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Test User 1", result[0].Name);
            Assert.AreEqual("Test User 2", result[1].Name);

            // Verifica se o método GetById foi chamado com os IDs corretos
            _userRepositoryMock.Verify(repo => repo.GetById(user1.Id), Times.Once);
            _userRepositoryMock.Verify(repo => repo.GetById(user2.Id), Times.Once);
        }

        [Test]
        public async Task CreateOng_ShouldCreateOng()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var name = "ONG Teste";
            var description = "Descrição Teste";
            var about = "Sobre Teste";
            var site = "www.teste.com";
            var mail = "teste@teste.com";
            var phone = "123456789";
            var instagram = "123456789";
            var youtube = "123456789";
            var street = "Rua Teste";
            var city = "Cidade Teste";
            var state = "Estado Teste";
            var zipCode = "00000-000";
            var country = "País Teste";
            var dict = new Dictionary<Stream, string>() { { new MemoryStream(), ".jpg" } };

            _ongRepositoryMock.Setup(repo => repo.Add(It.IsAny<Ong>())).Returns(Task.CompletedTask);

            // Act
            await _userService.CreateOng(
                ownerId, categoryId, name, description, about, site, mail, 
                phone, instagram, youtube, dict, street, city, state, zipCode, country);

            // Assert
            _ongRepositoryMock.Verify(repo => repo.Add(It.Is<Ong>(
                o => o.Name == name &&
                     o.Description == description &&
                     o.About == about &&
                     o.Site == site &&
                     o.Mail == mail &&
                     o.Phone == phone)), Times.Once);
        }

        [Test]
        public async Task UpdateOng_ShouldUpdateOng()
        {
            // Arrange
            var existingOng = new Ong(
                "ONG Teste", "Descrição Antiga", "Sobre Antigo", "www.antigo.com", "antigo@teste.com", "987654321", 
                "instagram", "youtube", "banner.key",
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            _ongRepositoryMock.Setup(repo => repo.GetById(existingOng.Id)).ReturnsAsync(existingOng);
            _ongRepositoryMock.Setup(repo => repo.Update(It.IsAny<Ong>())).Returns(Task.CompletedTask);

            var categoryId = Guid.NewGuid();
            var name = "ONG Atualizada";
            var description = "Descrição Atualizada";
            var about = "Sobre Atualizado";
            var site = "www.atualizado.com";
            var mail = "atualizado@teste.com";
            var phone = "123456789";
            var instagram = "instagram";
            var youtube = "youtube";
            var street = "Rua Atualizada";
            var city = "Cidade Atualizada";
            var state = "Estado Atualizado";
            var zipCode = "11111-111";
            var country = "País Atualizado";

            // Act
            await _userService.UpdateOng(existingOng.Id, categoryId, 
                name, description, about, site, mail, 
                phone, instagram, youtube, street, city, state, zipCode, country);

            // Assert
            _ongRepositoryMock.Verify(repo => repo.Update(It.Is<Ong>(
                o => o.Name == name &&
                     o.Description == description &&
                     o.About == about &&
                     o.Site == site &&
                     o.Mail == mail &&
                     o.Phone == phone
            )), Times.Once);
        }

        [Test]
        public async Task UpdateOngBanner_ShouldUpdateBanner()
        {
            // Arrange
            var ongId = Guid.NewGuid();
            var stream = new MemoryStream();
            string extension = ".jpg";

            _ongRepositoryMock.Setup(repo => repo.GetById(ongId)).ReturnsAsync(
                new Ong(
                "ONG Teste", "Descrição Antiga", "Sobre Antigo", "www.antigo.com", "antigo@teste.com", "987654321",
                "instagram", "youtube", "banner.key",
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

            // Act
            await _userService.UpdateOngBanner(ongId, stream, extension);

            // Assert
            _s3ServiceMock.Verify(s3 => s3.SendStreamFileToS3(stream, extension), Times.Once);
        }

        [Test]
        public async Task SearchByName_ShouldReturnUsersWithMatchingName()
        {
            // Arrange
            string name = "User";
            var mockUsers = new List<User> { new("Test User 1", "test@example.com", "hashedPassword", "documentId", AuthProvider.None,
                UserType.Common) };

            _userRepositoryMock.Setup(repo => repo.SearchByName(name)).ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.SearchByName(name);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test User 1", result[0].Name);
            _userRepositoryMock.Verify(repo => repo.SearchByName(name), Times.Once);
        }

        [Test]
        public async Task ListAll_ShouldReturnUsersWithPagination()
        {
            // Arrange
            int page = 1;
            int pageSize = 5;
            var mockUsers = new PagedResult<User> {
                Items = new List<User>()
                {
                    new("Test User 1", "test@example.com", "hashedPassword", "documentId", AuthProvider.None,
                    UserType.Common),

                    new("Test User 2", "test@example.com", "hashedPassword", "documentId", AuthProvider.None,
                    UserType.Common)
                }
            };

            _userRepositoryMock.Setup(repo => repo.GetWithFilter(null, UserType.Common, page, pageSize)).ReturnsAsync(mockUsers);

            // Act
            var result = await _userService.ListAll(null, UserType.Common, page, pageSize);

            // Assert
            Assert.AreEqual(2, result.Items.Count);
            _userRepositoryMock.Verify(repo => repo.GetWithFilter(null, UserType.Common, page, pageSize), Times.Once);
        }


        [Test]
        public async Task UpdateAvatar_UserExists_UpdatesAvatar()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var stream = new MemoryStream(); // Simula um stream de imagem
            var extension = ".jpg";
            var user = new User("Test User", "test@example.com", "hashedPassword", "documentId", AuthProvider.None,
                UserType.Common)
            {
                Id = userId,
                AvatarKey = "oldAvatarKey"
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            _s3ServiceMock.Setup(s3 => s3.SendStreamFileToS3(stream, extension)).ReturnsAsync("newAvatarKey");
            _s3ServiceMock.Setup(s3 => s3.DeleteFileByFileNameKey("oldAvatarKey")).Returns(Task.CompletedTask);
            _s3ServiceMock.Setup(s3 => s3.GetFileUrlByFileNameKey("newAvatarKey"))
                          .ReturnsAsync("https://example.com/newAvatarKey");
            // Act
            var avatarUrl = await _userService.UpdateAvatar(userId, stream, extension);

            // Assert
            Assert.AreEqual("newAvatarKey", user.AvatarKey);
            _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
            Assert.AreEqual("https://example.com/newAvatarKey", avatarUrl);
        }

        [Test]
        public async Task UpdateAvatar_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var stream = new MemoryStream();
            var extension = ".jpg";

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () => await _userService.UpdateAvatar(userId, stream, extension));
            Assert.AreEqual("Usuário inexistente", exception.Message);
        }

        [Test]
        public async Task UpdateAddress_UserExists_UpdatesExistingAddress()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("Test User", "test@example.com", "hashedPassword", "documentId", AuthProvider.None, UserType.Common)
            {
                Id = userId,
                AddressId = Guid.NewGuid()
            };

            var address = new Address("Old Street", "Old City", "Old State", "00000-000", "Old Country")
            {
                Id = Guid.Parse($"{user.AddressId}")
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);
            _addressRepositoryMock.Setup(repo => repo.GetById(Guid.Parse($"{user.AddressId}"))).ReturnsAsync(address);

            // Act
            await _userService.UpdateAddress(userId, "New Street", "New City", "New State", "11111-111", "New Country");

            // Assert
            Assert.AreEqual("New Street", address.Street);
            Assert.AreEqual("New City", address.City);
            _addressRepositoryMock.Verify(repo => repo.Update(address), Times.Once);
        }

        [Test]
        public async Task UpdateAddress_UserDoesNotHaveAddress_AddsNewAddress()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("Test User", "test@example.com", "hashedPassword", "documentId", AuthProvider.None, UserType.Common)
            {
                Id = userId,
                AddressId = null
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(user);

            // Act
            await _userService.UpdateAddress(userId, "New Street", "New City", "New State", "11111-111", "New Country");

            // Assert
            _addressRepositoryMock.Verify(repo => repo.Add(It.Is<Address>(a => a.Street == "New Street")), Times.Once);
            Assert.IsNotNull(user.AddressId);
        }

        [Test]
        public async Task Create_ShouldAddNewUser_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "test@example.com";
            var name = "Test User";
            var password = "Test123!";
            var documentId = "123456789";

            _userRepositoryMock.Setup(repo => repo.EmailExists(email)).ReturnsAsync(false);
            _utilityServiceMock.Setup(utility => utility.CryptSHA256(password)).Returns("hashed_password");

            // Simula o retorno do grupo de acesso "User" ao buscar pelo nome
            var groupAccess = new GroupAccess("User", "Acesso padrão ao sistema") { Id = Guid.NewGuid() };
            _groupAccessRepositoryMock.Setup(repo => repo.GetByName("User")).ReturnsAsync(groupAccess);

            // Act
            await _userService.Create(email, name, password, documentId);

            // Assert
            _userRepositoryMock.Verify(repo => repo.Add(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void Create_ShouldThrowException_WhenUserAlreadyExists()
        {
            // Arrange
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.EmailExists(email)).ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _userService.Create(email, "Test User", "Test123!", "123456789"));
            Assert.That(ex.Message, Is.EqualTo("Usuário já existe."));
        }

        [Test]
        public async Task Update_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("Old Name", "old@example.com", "old_password", "123456789", AuthProvider.None, UserType.Common)
            {
                Id = userId
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(existingUser);

            // Act
            var updatedUser = await _userService.Update(userId, "new@example.com", "New Name", "55119999999");

            // Assert
            Assert.AreEqual("new@example.com", updatedUser.Email);
            Assert.AreEqual("New Name", updatedUser.Name);
            _userRepositoryMock.Verify(repo => repo.Update(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public void Update_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync((User)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _userService.Update(userId, "new@example.com", "New Name", "55119999999"));
            Assert.That(ex.Message, Is.EqualTo("Usuário não encontrado."));
        }

        [Test]
        public async Task UpdatePassword_ShouldUpdatePassword_WhenOldPasswordMatches()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("Test User", "test@example.com", "old_hashed_password", "123456789", AuthProvider.None, UserType.Common)
            {
                Id = userId
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(existingUser);
            _utilityServiceMock.Setup(utility => utility.CryptSHA256("old_password")).Returns("old_hashed_password");
            _utilityServiceMock.Setup(utility => utility.CryptSHA256("new_password")).Returns("new_hashed_password");

            // Act
            await _userService.UpdatePassword(userId, "old_password", "new_password");

            // Assert
            _userRepositoryMock.Verify(repo => repo.Update(It.Is<User>(u => u.Password == "new_hashed_password")), Times.Once);
        }

        [Test]
        public void UpdatePassword_ShouldThrowException_WhenOldPasswordDoesNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User("Test User", "test@example.com", "old_hashed_password", "123456789", AuthProvider.None, UserType.Common)
            {
                Id = userId
            };

            _userRepositoryMock.Setup(repo => repo.GetById(userId)).ReturnsAsync(existingUser);
            _utilityServiceMock.Setup(utility => utility.CryptSHA256("old_password")).Returns("wrong_hashed_password");

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _userService.UpdatePassword(userId, "old_password", "new_password"));
            Assert.That(ex.Message, Is.EqualTo("Senha antiga incorreta"));
        }
    }
}

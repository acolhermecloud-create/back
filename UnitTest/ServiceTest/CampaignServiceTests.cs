using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;
using Microsoft.Extensions.Configuration;
using Moq;
using Service;

namespace UnitTest.ServiceTest
{
    [TestFixture]
    public class CampaignServiceTests
    {
        private CampaignService _campaignService;
        private Mock<ICampaignRepository> _mockCampaignRepositoryMock;
        private Mock<ICampaignDonationRepository> _mockCampaignDonationRepository;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IComplaintRepository> _mockComplaintRepository;
        private Mock<IPlanRepository> _mockPlanRepository;
        private Mock<IS3Service> _s3ServiceMock;
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IUserService> _mockUserService;
        private Mock<IUtilityService> _mockUtilityService;

        private Mock<IStoreService> _storeMock;
        private Mock<ILeverageRequestService> _leverageRequestMock;
        private Mock<IBankService> _bankMock;
        private Mock<IEmailService> _emailMock;
        private Mock<ICacheService> _cacheMock;

        /*

        IStoreService storeService,
        ILeverageRequestService leverageRequestService,
        IBankService bankService,
        IEmailService emailService,
        ICacheService cacheService

         */

        [SetUp]
        public void Setup()
        {
            _mockCampaignRepositoryMock = new Mock<ICampaignRepository>();
            _mockCampaignDonationRepository = new Mock<ICampaignDonationRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockComplaintRepository = new Mock<IComplaintRepository>();
            _mockPlanRepository = new Mock<IPlanRepository>();
            _s3ServiceMock = new Mock<IS3Service>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _configurationMock = new Mock<IConfiguration>();
            _mockUserService = new Mock<IUserService>();
            _mockUtilityService = new Mock<IUtilityService>();

            _storeMock = new Mock<IStoreService>();
            _leverageRequestMock = new Mock<ILeverageRequestService>();
            _bankMock = new Mock<IBankService>();
            _emailMock = new Mock<IEmailService>();
            _cacheMock = new Mock<ICacheService>();


            _campaignService = new CampaignService(
                _mockCampaignRepositoryMock.Object,
                _mockCampaignDonationRepository.Object,
                _mockCategoryRepository.Object,
                _mockComplaintRepository.Object,
                _configurationMock.Object,
                _s3ServiceMock.Object,
                _paymentServiceMock.Object,
                _mockUserService.Object,
                _mockPlanRepository.Object);
        }

        [Test]
        public async Task Report_ShouldAddComplaint()
        {
            // Arrange
            var campaignId = Guid.NewGuid();
            var denunciatorId = Guid.NewGuid();
            var iAm = "Não sou um doador desta vaquinha";
            var ARespectFor = "Violação dos termos de uso do Vakinha";
            var why = "Sou o beneficiário, mas estou removendo a autorização";
            var description = "Descrição da denúncia";

            // Act
            await _campaignService.Report(campaignId, denunciatorId, iAm, ARespectFor, why, description);

            // Assert
            _mockComplaintRepository.Verify(x => x.Add(It.IsAny<Complaint>()), Times.Once);
        }

        [Test]
        public async Task RemoveReportId_ShouldRemoveComplaint()
        {
            // Arrange
            var reportId = Guid.NewGuid();

            // Act
            await _campaignService.RemoveReportById(reportId);

            // Assert
            _mockComplaintRepository.Verify(x => x.RemoveById(reportId), Times.Once);
        }

        [Test]
        public async Task UpdateReportId_ShouldUpdateComplaint()
        {
            // Arrange
            var campaignId = Guid.NewGuid();
            var existingComplaint = new Complaint(campaignId, Guid.NewGuid(), "Não sou um doador desta vaquinha",
                "Violação dos termos de uso do Vakinha", "Sou o beneficiário, mas estou removendo a autorização", "Old description");

            _mockComplaintRepository.Setup(x => x.GetById(existingComplaint.Id)).ReturnsAsync(existingComplaint);

            Complaint updatedComplaint = null;

            _mockComplaintRepository.Setup(repo => repo.Update(It.IsAny<Complaint>()))
                                    .Callback<Complaint>(complaint => updatedComplaint = complaint)
                                    .Returns(Task.CompletedTask);

            var newDescription = "Updated description";
            var newStatus = ComplaintStatus.Closed;

            // Act
            await _campaignService.UpdateReportById(existingComplaint.Id, existingComplaint.IAm,
                existingComplaint.ARespectFor, existingComplaint.Why, newDescription);

            // Assert
            Assert.NotNull(updatedComplaint);
            Assert.AreEqual(newDescription, updatedComplaint.Description);
        }


        [Test]
        public async Task GetAllReports_ShouldReturnPagedReportsWithCampaigns()
        {
            var campaignId = Guid.NewGuid();

            // Arrange
            var complaints = new List<Complaint>
            {
                new Complaint(campaignId, Guid.NewGuid(), "Não sou um doador desta vaquinha",
                "Violação dos termos de uso do Vakinha", "Sou o beneficiário, mas estou removendo a autorização", "Old description"),
                new Complaint(campaignId, Guid.NewGuid(), "Não sou um doador desta vaquinha",
                "Violação dos termos de uso do Vakinha", "Sou o beneficiário, mas estou removendo a autorização", "Old description")
            };
            var categoryId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var oldTitle = "Old Title";
            var slug = "old-title";

            _mockComplaintRepository.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(complaints);
            _mockCampaignRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>())).ReturnsAsync(new 
                Campaign(Guid.NewGuid(), categoryId, planId, oldTitle, "Old Description", slug, 10000, "", CampaignisForWho.Me, DateTime.Now,
                new List<string> { "existing_image_key" }, planId, 7f));

            // Act
            var result = await _campaignService.GetAllReports(1, 10);

            // Assert
            Assert.AreEqual(2, result.Count);
            _mockComplaintRepository.Verify(x => x.Get(1, 10), Times.Once);
            _mockCampaignRepositoryMock.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Exactly(complaints.Count));
        }

        [Test]
        public async Task MakeDonation_Should_AddDonation()
        {
            // Arrange

            DonationRequest request = new();
            request.Amount = 1;
            request.Value = 10000;
            request.ClientIp = "";
            request.DonorId = Guid.NewGuid();
            request.CampaignId = Guid.NewGuid();
            request.DonateAt = DateTime.Now;
            request.TransationMethod = TransationMethod.Cash;
            request.DonationType = DonationType.Money;

            // Act
            await _campaignService.
                GeneratePaymentData(request);

            // Assert
            _mockCampaignDonationRepository.Verify(repo => repo.Add(It.IsAny<Donation>()), Times.Once);
        }

        [Test]
        public async Task GetCampaignsByDonor_Should_ReturnCampaignsForDonor()
        {
            // Arrange
            var donorId = Guid.NewGuid();
            var category = new Category("teste", "teste");
            var plan = new Plan("teste", "teste", new string[]{ "" }, 7f, false, true, 0);
            var slug = "old-title";

            var campaign = new Campaign(Guid.NewGuid(), category.Id, plan.Id, "Old Title", "Old Description", slug, 10000, "", CampaignisForWho.Me, DateTime.Now,
                new List<string> { "existing_image_key" }, plan.Id, 7f);

            var donations = new List<Donation>
            {
                new (campaign.Id, donorId, "asd", DonationType.Money, TransationMethod.Cash, 10000,
                1,
                DateTime.Now, DonationStatus.Created, DonationBalanceStatus.None, Gateway.ReflowPay)
            };
            
            _mockCampaignDonationRepository
                .Setup(repo => repo.GetByDonorId(donorId, 0, int.MaxValue))
                .ReturnsAsync(donations);

            _mockCategoryRepository
                .Setup(repo => repo.GetById(category.Id))
                .ReturnsAsync(category);

            _mockPlanRepository
                .Setup(repo => repo.GetDefault())
                .ReturnsAsync(plan);

            _mockCampaignRepositoryMock
                .Setup(repo => repo.GetById(campaign.Id))
                .ReturnsAsync(campaign);

            // Act
            var result = await _campaignService.GetCampaignsByDonor(donorId, 0, int.MaxValue);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(campaign.Id, result[0].Id);
        }

        [Test]
        public async Task GetDonationsByCampaign_Should_ReturnDonationsForCampaign()
        {
            // Arrange
            var donorId = Guid.NewGuid();
            var campaignId = Guid.NewGuid();

            var donations = new List<Donation>
            {
                new (campaignId, donorId, "asd", DonationType.Money, TransationMethod.Cash, 10000,
                1,
                DateTime.Now, DonationStatus.Created, DonationBalanceStatus.None, Gateway.ReflowPay)
            };

            _mockCampaignDonationRepository
                .Setup(repo => repo.GetByCampaignId(campaignId, 0, int.MaxValue))
                .ReturnsAsync(donations);

            // Act
            var result = await _campaignService.GetDonationsByCampaign(campaignId, 0, int.MaxValue);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(100m, result[0].Value);
        }

        [Test]
        public async Task GetExtractDonationById_Should_ReturnDonation()
        {
            // Arrange
            var campaignId = Guid.NewGuid();
            var donorId = Guid.NewGuid();
            var donationId = Guid.NewGuid();
            Donation donation = new(campaignId, donorId, "asd", DonationType.Money, TransationMethod.Cash, 10000,
                1,
                DateTime.Now, DonationStatus.Created, DonationBalanceStatus.None, Gateway.ReflowPay);

            _mockCampaignDonationRepository
                .Setup(repo => repo.GetById(donationId))
                .ReturnsAsync(donation);

            // Act
            var result = await _campaignService.GetExtractDonationById(donationId);

            // Assert
            Assert.AreEqual(100.0m, result.Value);
        }

        [Test]
        public async Task AddImages_ValidImages_ShouldReturnUpdatedMediaList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var slug = "old-title";

            var existingCampaign = new Campaign(Guid.NewGuid(), categoryId, planId, "Old Title", "Old Description", slug, 10000, "", CampaignisForWho.Me, DateTime.Now,
                new List<string> { "existing_image_key" }, planId, 7f);

            var newFilesAndExtensions = new Dictionary<Stream, string>();
            var fileStreamMock = new Mock<Stream>();
            newFilesAndExtensions.Add(fileStreamMock.Object, "png");

            _mockCampaignRepositoryMock.Setup(repo => repo.GetById(existingCampaign.Id))
                                   .ReturnsAsync(existingCampaign);

            _s3ServiceMock.Setup(s => s.SendStreamFileToS3(It.IsAny<Stream>(), It.IsAny<string>()))
                          .ReturnsAsync("new_image_key");

            _configurationMock.SetupGet(config => config["System:MaxImagePeerCampaign"])
                              .Returns("5");

            // Act
            var updatedMediaList = await _campaignService.AddImages(existingCampaign.Id, newFilesAndExtensions);

            // Assert
            Assert.AreEqual(2, updatedMediaList.Count);
            Assert.Contains("existing_image_key", updatedMediaList);
            Assert.Contains("new_image_key", updatedMediaList);
            _mockCampaignRepositoryMock.Verify(repo => repo.Update(existingCampaign), Times.Once);
        }

        [Test]
        public async Task RemoveImages_ValidImageKeys_ShouldUpdateMediaList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var slug = "old-title";

            var existingCampaign = new Campaign(Guid.NewGuid(), categoryId, planId, "Old Title", "Old Description", slug, 10000, "", CampaignisForWho.Me, DateTime.Now,
                new List<string> { "existing_image_key" }, planId, 7f);

            var imagesToRemove = new List<string> { "image_key_1" };

            _mockCampaignRepositoryMock.Setup(repo => repo.GetById(existingCampaign.Id))
                                   .ReturnsAsync(existingCampaign);

            _s3ServiceMock.Setup(s => s.DeleteFileByFileNameKey(It.IsAny<string>()))
                          .Returns(Task.CompletedTask);

            // Act
            await _campaignService.RemoveImages(existingCampaign.Id, imagesToRemove);

            // Assert
            Assert.AreEqual(1, existingCampaign.Media.Count);
            Assert.IsFalse(existingCampaign.Media.Contains("image_key_1"));
            Assert.IsTrue(existingCampaign.Media.Contains("image_key_2"));
            _mockCampaignRepositoryMock.Verify(repo => repo.Update(existingCampaign), Times.Once);
            _s3ServiceMock.Verify(s => s.DeleteFileByFileNameKey("image_key_1"), Times.Once);
        }

        [Test]
        public async Task CreateCampaign_ValidData_ShouldReturnCampaign()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var fileStreams = new Dictionary<Stream, string>();
            var fileStreamMock = new Mock<Stream>();
            fileStreams.Add(fileStreamMock.Object, "jpg");

            var plan = new Plan("teste", "teste", new string[] { "" }, 7f, false, true, 0);

            _s3ServiceMock.Setup(s => s.SendStreamFileToS3(It.IsAny<Stream>(), It.IsAny<string>()))
                          .ReturnsAsync("file_key");

            _mockCampaignRepositoryMock.Setup(repo => repo.Add(It.IsAny<Campaign>()))
                                   .Returns(Task.CompletedTask);

            _mockPlanRepository
                .Setup(repo => repo.GetDefault())
                .ReturnsAsync(plan);

            // Act
            var campaign = await _campaignService.Create("Title", "Description", 1000, "", CampaignisForWho.Me, DateTime.Now.AddDays(30), categoryId, userId, fileStreams);

            // Assert
            Assert.IsNotNull(campaign);
            Assert.AreEqual("Title", campaign.Title);
            Assert.AreEqual(userId, campaign.CreatorId);
            Assert.AreEqual(categoryId, campaign.CategoryId);
            _s3ServiceMock.Verify(s => s.SendStreamFileToS3(It.IsAny<Stream>(), "jpg"), Times.Once);
        }

        [Test]
        public async Task UpdateCampaign_ValidData_ShouldReturnUpdatedCampaign()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var planId = Guid.NewGuid();
            var slug = "old-title";

            var existingCampaign = new Campaign(Guid.NewGuid(), categoryId, planId, "Old Title", "Old Description", slug, 10000, "", CampaignisForWho.Me, DateTime.Now,
                new List<string> { "existing_image_key" }, planId, 7f);

            _mockCampaignRepositoryMock.Setup(repo => repo.GetById(existingCampaign.Id))
                                   .ReturnsAsync(existingCampaign);

            _mockCampaignRepositoryMock.Setup(repo => repo.Update(It.IsAny<Campaign>()))
                                   .Returns(Task.CompletedTask);

            // Act
            var updatedCampaign = await _campaignService.Update(existingCampaign.Id, "New Title", "New Description", 1000, DateTime.Now.AddDays(20), categoryId);

            // Assert
            Assert.IsNotNull(updatedCampaign);
            Assert.AreEqual("New Title", updatedCampaign.Title);
            Assert.AreEqual("New Description", updatedCampaign.Description);
            _mockCampaignRepositoryMock.Verify(repo => repo.Update(existingCampaign), Times.Once);
        }

        [Test]
        public void UpdateCampaign_CampaignNotFound_ShouldThrowException()
        {
            // Arrange
            var campaignId = Guid.NewGuid();
            _mockCampaignRepositoryMock.Setup(repo => repo.GetById(campaignId))
                                   .ReturnsAsync((Campaign)null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _campaignService.Update(campaignId, "Title", "Description", 10000, DateTime.Now.AddDays(20), Guid.NewGuid()));
        }
    }
}

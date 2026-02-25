using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using Util;

namespace Service
{
    public class CampaignService(
        ICampaignRepository campaignRepository,
        ICampaignDonationRepository campaignDonationRepository,
        ICategoryRepository categoryRepository, 
        IComplaintRepository complaintRepository,
        ICampaignLogsRepository campaignLogsRepository,
        ICampaignCommentsRepository campaignCommentsRepository,
        IConfiguration configuration, 
        IS3Service s3Service,
        IPaymentService paymentService,
        IUserService userService,
        IPlanRepository planRepository,
        IUtilityService utilityService,
        IStoreService storeService,
        ILeverageRequestService leverageRequestService,
        IBankService bankService,
        IEmailService emailService,
        ICacheService cacheService,
        IUtmRepository utmRepository,
        IUtmfyService utmfyService) : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly ICampaignDonationRepository _campaignDonationRepository = campaignDonationRepository;
        private readonly ICampaignLogsRepository _campaignLogsRepository = campaignLogsRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IComplaintRepository _complaintRepository = complaintRepository;
        private readonly IPlanRepository _planRepository = planRepository;
        private readonly ICampaignCommentsRepository _campaignCommentsRepository = campaignCommentsRepository;

        private readonly IConfiguration _configuration = configuration;

        private readonly IUserService _userService = userService;
        private readonly IS3Service _s3Service = s3Service;
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IUtilityService _utilityService = utilityService;
        private readonly IStoreService _storeService = storeService;
        private readonly ILeverageRequestService _leverageRequestService = leverageRequestService;
        private readonly ICacheService _cacheService = cacheService;
        private readonly IUtmfyService _utmfyService = utmfyService;

        private readonly IBankService _bankService = bankService;
        private readonly IEmailService _emailService = emailService;

        private readonly IUtmRepository _utmRepository = utmRepository;

        public async Task<Campaign> Create(string title, string description, decimal goal, string beneficiaryName, CampaignisForWho forWho,
            DateTime dueDate, Guid categoryId, Guid userId, Dictionary<Stream, string> FilesAndExtensions)
        {
            var defaultPlan = await _planRepository.GetDefault();
            if (defaultPlan == null)
                throw new Exception("Não existe um plano padrão cadastrado");

            List<string> fileKeys = new List<string>();
            foreach (var file in FilesAndExtensions)
            {
                var filenamekey = await _s3Service.SendStreamFileToS3(file.Key, file.Value);
                if (!string.IsNullOrEmpty(filenamekey))
                    fileKeys.Add(filenamekey);
            }
            string baseSlug = _utilityService.GenerateSlug(title);
            string uniqueSlug = baseSlug;
            int counter = 1;
            while (await campaignRepository.ExistSlug(uniqueSlug))
            {
                uniqueSlug = $"{baseSlug}-{counter}";
                counter++;
            }

            var campaign = new Campaign(userId, categoryId, defaultPlan.Id, title, description, uniqueSlug, goal, 
                beneficiaryName, forWho, dueDate, fileKeys, defaultPlan.Id, defaultPlan.PercentToBeCharged, defaultPlan.FixedRate);
            await _campaignRepository.Add(campaign);

            var userrepo = await _userService.GetById(userId);
            await _bankService.CreateAccount(userId, BankAccountType.Customer, userrepo?.DocumentId);

            //await _paymentService.CreateSubAccount(userId);
            return campaign;
        }

        public async Task<Campaign> Update(Guid id, string title, string description, decimal goal, DateTime dueDate, Guid categoryId)
        {
            var campaign = await _campaignRepository.GetById(id) ?? throw new Exception("Campanha não encontrada.");

            if(campaign.FinancialGoal != goal)
            {
                var c = new CultureInfo("pt-BR");
                var oldFinancialGoal = (campaign.FinancialGoal / 100).ToString("C", c);
                var newFinancialGoal = (goal / 100).ToString("C", c);

                CampaignLogs log = new(id, $"De {oldFinancialGoal} para {newFinancialGoal}", CampaignLogType.ChangePrice);
                await _campaignLogsRepository.Add(log);
            }

            campaign.Title = title;
            campaign.Description = description;
            campaign.FinancialGoal = goal;
            campaign.CreatedAt = dueDate;
            campaign.CategoryId = categoryId;
            await _campaignRepository.Update(campaign);

            return campaign;
        }

        public async Task UpdateListingCampaign(Guid id, bool listing)
        {
            var campaign = await _campaignRepository.GetById(id) ?? throw new Exception("Campanha não encontrada.");
            campaign.Listing = listing;
            campaign.UpdateAt = DateTime.Now;

            await _campaignRepository.Update(campaign);
        }
        public async Task UpdateStatus(Guid id, CampaignStatus status, string? reason)
        {
            var campaignrepository = await _campaignRepository.GetById(id);
            if(campaignrepository != null)
            {
                campaignrepository.Status = status;
                campaignrepository.Reason = reason;
                campaignrepository.UpdateAt = DateTime.Now;
                await _campaignRepository.Update(campaignrepository);

                CampaignLogs log = new(id, $"Mudança de status", CampaignLogType.ChangeStatus);
                await _campaignLogsRepository.Add(log);
            }
        }

        public async Task Delete(Guid id)
        {
            var campaign = await _campaignRepository.GetById(id) ?? throw new Exception("Campanha não encontrada.");
            foreach (var imageKey in campaign.Media)
            {
                try
                {
                    await _s3Service.DeleteFileByFileNameKey(imageKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao deletar imagem {imageKey}: {ex.Message}");
                }
            }
            await _campaignRepository.Delete(id);
        }

        public async Task<Campaign> GetById(Guid id, int page, int pageSize)
        {
            var campaign = await _campaignRepository.GetById(id);
            campaign.Donations = await GetDonations(campaign.Id, 1, int.MaxValue);
            campaign.Category = await _categoryRepository.GetById(campaign.CategoryId);
            campaign.Comments = await _campaignCommentsRepository.ListByCampaignId(campaign.Id);

            return campaign ?? throw new Exception("Campanha não encontrada.");
        }

        public async Task<PagedResult<Campaign>> GetFilteredCampaigns(
            DateTime? startDate = null, DateTime? endDate = null, Guid? categoryId = null, string? title = null,
            List<Guid>? campaignIds = null,
            bool? listing = true,
            CampaignStatus status = CampaignStatus.Active,
            int page = 1, int pageSize = 10)
        {
            var campaigns = await _campaignRepository.GetCampaigns(
                startDate, endDate, categoryId, title, [], status, listing, page, pageSize);

            // Ordenando as campanhas pela quantidade de doações do tipo 1
            var newOrdenation = campaigns.Items
            .OrderByDescending(x => x.Donations
                .Where(d => d.Type == DonationType.SmallDonations && d.Status == DonationStatus.Paid) // Filtra as doações com as condições
                .Sum(d => d.Amount)) // Soma o valor das doações filtradas
            .ToList();
            // Converte para lista

            // Atualiza a lista de campanhas ordenadas
            campaigns.Items = newOrdenation;

            return campaigns;
        }

        public async Task<PagedResult<Campaign>> GetFilteredCampaigns(DateTime? startDate, DateTime? endDate, string? name, 
            CampaignStatus? status, bool? listing, int page, int pageSize)
        {
            var campaigns = await _campaignRepository.GetCampaigns(
                startDate, endDate, null, null, [], status, listing, page, pageSize);

            // Ordenando as campanhas pela quantidade de doações do tipo 1
            var newOrdenation = campaigns.Items
                .OrderByDescending(x => x.Donations
                    .Where(d => d.Type == DonationType.SmallDonations && d.Status == DonationStatus.Paid) // Filtra as doações com as condições
                    .Sum(d => d.Amount)) // Soma o valor das doações filtradas
                .ToList();
            // Converte para lista

            // Atualiza a lista de campanhas ordenadas
            campaigns.Items = newOrdenation;

            return campaigns;
        }

        public async Task<List<Donation>> GetDonations(Guid campaignId, int page, int pageSize)
        {
            var donations = await _campaignDonationRepository.GetByCampaignId(campaignId, page, pageSize);
            foreach (var donation in donations)
            {
                var donor = await _userService.GetById(donation.DonorId);
                if(donor != null)
                    donation.Donor = new() { Name = donor.Name, AvatarKey = donor.AvatarKey };
            }

            return donations;
        }

        public async Task<List<string>> AddImages(Guid id, Dictionary<Stream, string> newFilesAndExtensions)
        {
            var campaign = await _campaignRepository.GetById(id);
            if (campaign == null) throw new Exception("Campanha inexistente");

            int maxCampaignImages = int.Parse(_configuration["System:MaxImagePeerCampaign"]);
            if ((campaign.Media.Count + newFilesAndExtensions.Count) > maxCampaignImages)
                throw new Exception("Número máximo de imagens atingido.");

            foreach (var file in newFilesAndExtensions)
            {
                var filenamekey = await _s3Service.SendStreamFileToS3(file.Key, file.Value);
                if (!string.IsNullOrEmpty(filenamekey))
                    campaign.Media.Add(filenamekey);
            }

            CampaignLogs log = new(id, $"Imagens da campanha atualizada", CampaignLogType.ChangeImages);
            await _campaignLogsRepository.Add(log);

            await _campaignRepository.Update(campaign);
            return campaign.Media;
        }

        public async Task RemoveImages(Guid id, List<string> imagesKeyToRemove)
        {
            var campaign = await _campaignRepository.GetById(id) ?? throw new Exception("Campanha inexistente");
            foreach (var imageKey in imagesKeyToRemove)
            {
                if (campaign.Media.Contains(imageKey))
                {
                    try
                    {
                        await _s3Service.DeleteFileByFileNameKey(imageKey);
                        campaign.Media.Remove(imageKey);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao deletar imagem {imageKey}: {ex.Message}");
                    }
                }
            }

            CampaignLogs log = new(id, $"Imagens da campanha atualizada", CampaignLogType.ChangeImages);
            await _campaignLogsRepository.Add(log);

            await _campaignRepository.Update(campaign);
        }

        public async Task MakeSmallDonation(Guid campaignId, Guid donorId, DonationType type, int amount)
        {
            var campaign = await _campaignRepository.GetById(campaignId);
            var user = await _userService.GetById(donorId);

            await _storeService.AddUsageDigitalStickers(donorId, campaignId, amount);

            var donation = new Donation(
                campaignId,
                donorId,
                Guid.NewGuid().ToString(),
                type,
                TransationMethod.Store,
                0,
                amount,
                DateTime.Now,
                DonationStatus.Paid,
                DonationBalanceStatus.None,
                Gateway.Internal);

            await _campaignDonationRepository.Add(donation);

            CampaignLogs log = new(campaignId, $"Campanha recebeu {amount} kaixinhas", CampaignLogType.ReceiveDonation);
            await _campaignLogsRepository.Add(log);
        }

        public async Task<TransactionData> GeneratePaymentData(DonationRequest request)
        {
            var timestamp = Functions.GenerateTimeStampStrUnique();

            bool userWasCreated = false;
            Guid donorId = Guid.Empty;

            var donorrepository = await _userService.GetByEmail(request.DonorEmail)
                ?? await _userService.GetByDocument(request.DonorDocumentId);

            if (donorrepository != null)
                donorId = donorrepository.Id;
            else
            {
                var userId = await _userService.GetOrCreate(
                    request.DonorEmail,
                    request.DonorName,
                    timestamp,
                    request.DonorDocumentId,
                    UserType.Common, request.DonorPhone);

                donorrepository = await _userService.GetById(Guid.Parse(userId));

                userWasCreated = true;
            }

            var campaign = await _campaignRepository.GetById(request.CampaignId);

            if (!campaign.CanReceiveDonation)
                throw new Exception("Essa campanha foi pausada, tente novamente mais tarde.");

            Domain.Objects.ReflowPay.Item item = new();
            item.Title = campaign.Title;
            item.Description = campaign.Title;
            item.UnitPrice = (int)request.Value;
            item.Quantity = 1;
            item.Tangible = false;

            var paymentinfo = await _paymentService.GeneratePix((int)request.Value, request.ClientIp, item, donorrepository, 
                Strings.Webhooks["donation"]);

            paymentinfo.TransactionSource = BankTransactionSource.Campaign;

            await _cacheService.Set(paymentinfo.Id, JsonConvert.SerializeObject(paymentinfo), 1);

            var donation = new Donation(
                request.CampaignId,
                donorId,
                paymentinfo.Id.ToString(),
                request.DonationType,
                request.TransationMethod,
                (int)request.Value,
                (int)request.Amount,
                request.DonateAt,
                DonationStatus.Created,
                DonationBalanceStatus.None,
                paymentinfo.Gateway);

            await _campaignDonationRepository.Add(donation);

            if(userWasCreated && paymentinfo?.QRCode != null)
            {
                var base64Stream = _utilityService.ConvertBase64ToStream(paymentinfo.QRCode);
                var imageKey = await _s3Service.SendStreamFileToS3(base64Stream, ".png");
                var qrCodeUrl = await _s3Service.GetFileUrlByFileNameKey(imageKey);

                _ = GenerateDonationEmail(campaign.Title, donorrepository.Name, donorrepository.Email, timestamp, qrCodeUrl);
            }
            
            return paymentinfo;
        }

        protected async Task GenerateDonationEmail(string campaignTitle, string name, string email, string password, string qrCodeBase64)
        {
            var subject = $"Parabéns por ter a iniciativa de fazer a doação para a causa {campaignTitle}";

            var htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='background-color: #ffffff; padding: 30px; border-radius: 8px; max-width: 600px; margin: auto;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <img src='https://www.kaixinha.io/_next/static/media/logo0.4bb15040.png' alt='Company Logo' style='height: 60px;' />
                        </div>

                        <h2>Olá, <span style='color: #ff6700;'>{name}</span>!</h2>
                        <p>👏 Parabéns pela sua iniciativa! Sua contribuição é um passo poderoso para transformar a causa <strong>{campaignTitle}</strong> em realidade.</p>

                        <h3>🔐 Sua senha de acesso ao sistema:</h3>
                        <p style='font-size: 14px; color: #333;'><strong>{password}</strong></p>

                        <h3>💳 Faça o pagamento via Pix:</h3>
                        <p>Use o QR Code abaixo para concluir sua doação:</p>
                        <img src='{qrCodeBase64}' alt='QR Code Pix' style='max-width: 100%; height: auto;' />

                        <p style='margin-top: 30px;'>Muito obrigado por apoiar essa causa. Juntos somos mais fortes!</p>
                    </div>
                </body>
            </html>
            ";

            string[] receivers = [email];
            string title = subject;
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            await _emailService.Send(receivers, title, htmlBody, businessAddresses,
                businessName, null);
        }

        public async Task ConfirmDonation(string transactionId)
        {
            var donationrepo = await _campaignDonationRepository.GetByTransactionId(transactionId);
            if(donationrepo != null && donationrepo.Status == DonationStatus.Created)
            {
                donationrepo.Status = DonationStatus.Paid;
                await _campaignDonationRepository.Update(donationrepo);

                await _paymentService.ConfirmTransaction(donationrepo.Gateway, transactionId);

                CampaignLogs log = new(donationrepo.CampaignId, $"Campanha recebeu uma doação", CampaignLogType.ReceiveDonation);
                await _campaignLogsRepository.Add(log);

                var campaignrepo = await _campaignRepository.GetById((Guid)donationrepo.CampaignId);
                var currentPlan = await _planRepository.GetById(campaignrepo.PlanId);

                decimal amountToBeDivided = donationrepo.Value / 100m;

                // SE NewPercentToBeCharged FOR DEFINIDO, ENTÃO DEVE-SE CONSIDERAR ELE AO INVES DO PLANO
                decimal percentToCharge = campaignrepo?.CurrentPercentToBeCharged != null ? (decimal)campaignrepo.CurrentPercentToBeCharged : currentPlan.PercentToBeCharged;
                decimal fixedRate = campaignrepo?.FixedRate != null ? (decimal)campaignrepo.FixedRate : currentPlan.FixedRate;

                await _bankService.MakeSplitToSystemAccount(
                    campaignrepo.CreatorId,
                    donationrepo.Id,
                    amountToBeDivided,
                    BankSplitType.Percent,
                    percentToCharge,
                    fixedRate);

                var utmrepository = await _utmRepository.GetByOrderId(transactionId);
                if(utmrepository != null)
                {
                    utmrepository.ApprovedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    utmrepository.Status = "paid";
                    await _utmRepository.Update(utmrepository);

                    _ = _utmfyService.SendEvent(utmrepository);
                }
            }
        }

        public async Task<bool> CheckDonation(string transactionId)
        {
            var donationrepo = await _campaignDonationRepository.GetByTransactionId(transactionId);
            if (donationrepo == null) return false;

            var payed = await _paymentService.ConfirmPix(donationrepo.Gateway, transactionId);

            return payed;
        }

        public async Task<List<Campaign>> GetCampaignsByDonor(Guid donorId, int page, int pageSize)
        {
            var donations = await _campaignDonationRepository.GetByDonorId(donorId, page, pageSize);

            List<Campaign> campaigns = [];
            foreach (var donation in donations)
            {
                var campaign = await _campaignRepository.GetById((Guid)donation.CampaignId);
                if (campaign != null)
                {
                    campaign.Category = await _categoryRepository.GetById(campaign.CategoryId);
                    campaign.Donations = await _campaignDonationRepository.GetByCampaignId(campaign.Id, 1, int.MaxValue);
                    campaigns.Add(campaign);
                }
            }

            return campaigns;
        }

        public async Task<List<Donation>> GetDonationsByCampaign(Guid campaignId, int page, int pageSize)
        {
            var donations = await _campaignDonationRepository.GetByCampaignId(campaignId, page, pageSize);

            foreach (var donation in donations)
            {
                var donor = await _userService.GetById(donation.DonorId);
                if(donor != null){
                    donor.Password = string.Empty;
                    donation.Donor = donor;
                }
            }

            return donations;
        }

        public async Task<Donation> GetExtractDonationById(Guid donationId)
        {
            return await _campaignDonationRepository.GetById(donationId);
        }

        public async Task<List<Campaign>> GetByUserId(Guid userId, int page, int pageSize)
        {
            var campaings = await _campaignRepository.GetByCreatorId(userId, page, pageSize);
            foreach (var campaign in campaings)
            {
                campaign.Category = await _categoryRepository.GetById(campaign.CategoryId);
                campaign.Donations = await _campaignDonationRepository.GetByCampaignId(campaign.Id, 1, int.MaxValue);
                campaign.LeverageRequest = await _leverageRequestService.GetByCampaignId(campaign.Id);
                campaign.Comments = await _campaignCommentsRepository.ListByCampaignId(campaign.Id);

                foreach (var donation in campaign.Donations)
                {
                    var donor = await _userService.GetById(donation.DonorId);
                    if (donor != null)
                        donor.Password = string.Empty;

                    donation.Donor = donor;
                }
            }

            return campaings;
        }

        public async Task Report(Guid campaignId, Guid? denunciatorId, 
            string iAm, 
            string aRespectFor,
            string why,
            string description)
        {
            var complaint = new Complaint(campaignId, denunciatorId,
                iAm,
                aRespectFor,
                why,
                description);

            await _complaintRepository.Add(complaint);
        }

        public async Task RemoveReportById(Guid reportId) => await _complaintRepository.RemoveById(reportId);

        public async Task UpdateReportById(Guid reportId,
            string iAm,
            string aRespectFor,
            string why,
            string description)
        {
            var report = await _complaintRepository.GetById(reportId);
            if (report == null)
                throw new Exception("Denuncia não existe");

            report.IAm = iAm;
            report.ARespectFor = aRespectFor;
            report.Why = why;
            report.Description = description;

            await _complaintRepository.Update(report);
        }

        public async Task<List<Complaint>> GetAllReports(int page, int pageSize)
        {
            var reports = await _complaintRepository.Get(page, pageSize);
            foreach (var report in reports)
            {
                var campaign = await _campaignRepository.GetById(report.CampaignId);
                if(campaign != null)
                    report.Campaign = campaign;
            }

            return reports;
        }

        public async Task<List<Category>> GetAllCategory()
        {
            return await _categoryRepository.GetAll();
        }

        public async Task<Campaign> GetBySlug(string slug)
        {
            var campaignrepo = await _campaignRepository.GetBySlug(slug);
            if (campaignrepo == null)
                return null;

            campaignrepo.Category = await _categoryRepository.GetById(campaignrepo.CategoryId);
            campaignrepo.Donations = await GetDonations(campaignrepo.Id, 1, int.MaxValue);
            campaignrepo.DigitalStickers = await _storeService.GetDigitalStickersByCampaignId(campaignrepo.Id);
            campaignrepo.Comments = await _campaignCommentsRepository.ListByCampaignId(campaignrepo.Id);

            foreach (var comment in campaignrepo.Comments)
                comment.UserName = (await _userService.GetById(comment.UserId)).Name;

            return campaignrepo;
        }

        public async Task<List<CampaignLogs>> GetLogs(Guid campaignId)
        {
            return await _campaignLogsRepository.GetByCampaignId(campaignId);
        }

        public async Task<BalanceExtractResume> GetUserBalanceAwaitingRelease(Guid userId)
        {
            return await _bankService.GetWaitingRelease(userId);
        }

        public async Task<BalanceExtractResume> GetUserBalanceReleasedForWithdraw(Guid userId)
        {
            return await _bankService.GetReleased(userId);
        }

        public async Task<List<Plan>> ListPlans()
        {
            return await _planRepository.GetAll();
        }

        public async Task<PagedResult<Plan>> ListPlans(int page, int pageSize)
        {
            return await _planRepository.GetAll(page, pageSize);
        }

        public async Task AddComment(Guid campaignId, Guid userId, string message)
        {
            CampaignComments comment = new(campaignId, userId, message, DateTime.Now);

            await _campaignCommentsRepository.Add(comment);
        }

        public async Task RemoveComment(Guid commentId)
        {
            await _campaignCommentsRepository.RemoveById(commentId);
        }


        #region JOBS

        public async Task CheckCampaignWithoutDonationsInTenDays()
        {
            var campaigns = await _campaignRepository.GetCampaignsByStatusAndMinAge(CampaignStatus.Active, DateTime.Now, 10);
            foreach (var campaign in campaigns)
            {
                var donations = await _campaignDonationRepository.GetByCampaignId(campaign.Id);
                if(donations == null || donations.Count == 0)
                {
                    var owner = await _userService.GetById(campaign.CreatorId);
                    if(owner != null)
                    {
                        string endDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy"); // Substitua pela data real

                        string subject = $"🚨 {owner.Name} - Temos um comunicado importante";
                        string body = $@"<html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Aviso Importante</title>
                            </head>
                            <body style='font-family: Arial, sans-serif;'>
                                <p>Oi, {owner.Name},</p>
                                <p>Aviso sobre a sua vaquinha.</p>
                                <p>Estamos entrando em contato sobre a vaquinha <strong>{campaign.Title}</strong>, criada no dia <strong>{campaign.CreatedAt:dd/MM/yyyy}</strong>, 
                                que não recebeu nenhuma doação desde o momento em que foi criada.</p>
                                <p>Por motivos de segurança, vaquinhas que não arrecadam nenhum valor em 30 dias desde a data da criação são automaticamente encerradas.</p>
                                <p>Caso não receba nenhuma doação, sua vaquinha será encerrada automaticamente no dia <strong>{endDate}</strong>.</p>
                                <br>
                                <h3>Quer continuar arrecadando?</h3>
                                <p>Divulgue sua vaquinha sem parar e consiga doações!</p>
                            </body>
                            </html>";

                        await SendMailNotification(subject, body, [owner.Email]);
                    }
                }
            }
        }

        public async Task CheckCampaignWithoutDonationsInTwentyDays()
        {
            var campaigns = await _campaignRepository.GetCampaignsByStatusAndMinAge(CampaignStatus.Active, DateTime.Now, 20);
            foreach (var campaign in campaigns)
            {
                var donations = await _campaignDonationRepository.GetByCampaignId(campaign.Id);
                if (donations == null || donations.Count == 0)
                {
                    var owner = await _userService.GetById(campaign.CreatorId);
                    if (owner != null)
                    {
                        string endDate = DateTime.Now.AddDays(10).ToString("dd/MM/yyyy");

                        string subject = $"⚠️ {owner.Name} - Sua vaquinha pode ser encerrada em breve";
                        string body = $@"<html>
                            <head>
                                <meta charset='UTF-8'>
                                <title>Aviso Final</title>
                            </head>
                            <body style='font-family: Arial, sans-serif;'>
                                <p>Oi, {owner.Name},</p>
                                <p>Aviso sobre a sua vaquinha.</p>
                                <p>A vaquinha <strong>{campaign.Title}</strong>, criada em <strong>{campaign.CreatedAt:dd/MM/yyyy}</strong>, 
                                ainda não recebeu nenhuma doação.</p>
                                <p>Se nenhuma doação for recebida até o dia <strong>{endDate}</strong>, sua vaquinha será automaticamente encerrada.</p>
                                <br>
                                <h3>Divulgue agora e evite o encerramento!</h3>
                                <p>Compartilhe o link e peça apoio.</p>
                            </body>
                            </html>";

                        await SendMailNotification(subject, body, [owner.Email]);
                    }
                }
            }
        }

        public async Task CloseInactiveCampaigns()
        {
            var campaigns = await _campaignRepository.GetCampaignsByStatusAndMinAge(CampaignStatus.Active, DateTime.Now, 30);
            foreach (var campaign in campaigns)
            {
                var donations = await _campaignDonationRepository.GetByCampaignId(campaign.Id);
                if (donations == null || donations.Count == 0)
                {
                    campaign.Status = CampaignStatus.Closed;
                    await _campaignRepository.Update(campaign);

                    var owner = await _userService.GetById(campaign.CreatorId);
                    if (owner != null)
                    {
                        string subject = $"🚫 {owner.Name} - Sua vaquinha foi encerrada";
                        string body = $@"<html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Vaquinha Encerrada</title>
                    </head>
                    <body style='font-family: Arial, sans-serif;'>
                        <p>Oi, {owner.Name},</p>
                        <p>Infelizmente, sua vaquinha <strong>{campaign.Title}</strong>, criada em <strong>{campaign.CreatedAt:dd/MM/yyyy}</strong>, 
                        não recebeu nenhuma doação e foi automaticamente encerrada.</p>
                        <p>Se precisar de ajuda para criar uma nova vaquinha, estamos à disposição!</p>
                        <br>
                        <h3>Precisa de suporte?</h3>
                        <p>Entre em contato conosco para mais informações.</p>
                    </body>
                    </html>";

                        await SendMailNotification(subject, body, [owner.Email]);
                    }
                }
            }
        }

        #endregion

        protected async Task SendMailNotification(string subject, string body, string[] recipients)
        {
            string[] receivers = recipients;
            string title = subject;
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            await _emailService.Send(receivers, title, body, businessAddresses,
                businessName, null);
        }

        public async Task RenamePropertie()
        {
            await _campaignRepository.RenameNewPlanIdToCurrentPlanIdAsync();
        }

        public async Task RecordUtm(Utm utm)
        {
            await _utmfyService.SendEvent(utm);

            var utmrepository = await _utmRepository.GetByOrderId(utm.OrderId);
            if (utmrepository == null)
                await _utmRepository.Add(utm);
            else
            {
                utmrepository.Status = utm.Status;
                utmrepository.ApprovedDate = utm.ApprovedDate;

                await _utmRepository.Update(utmrepository);
            }
        }
    }
}
using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;
using Microsoft.Extensions.Configuration;

namespace Service
{
    public class LeverageRequestService(
        ILeverageRequestRepository leverageRequestRepository,
        ICampaignRepository campaignRepository,
        IPlanRepository planRepository,
        IS3Service s3Service,
        IUserService userService,
        IEmailService emailService,
        IConfiguration configuration) : ILeverageRequestService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILeverageRequestRepository _leverageRequestRepository = leverageRequestRepository;
        private readonly ICampaignRepository _campaignRepository = campaignRepository;
        private readonly IPlanRepository _planRepository = planRepository;

        private readonly IS3Service _s3Service = s3Service;
        private readonly IUserService _userService = userService;
        private readonly IEmailService _emailService = emailService;

        public async Task ChangeStatus(Guid id, LeverageStatus leverageStatus)
        {
            var leverage = await _leverageRequestRepository.GetById(id);
            if (leverage == null) throw new Exception("Não foi possível localizar solicitação de upgrade");

            var plan = await _planRepository.GetById(leverage.PlanId);

            var campaign = await _campaignRepository.GetById(leverage.CampaignId);
            if (campaign == null) throw new Exception("Não foi possível localizar a campanha");

            var user = await _userService.GetById(campaign.CreatorId);

            string body = string.Empty;
            if (leverageStatus == LeverageStatus.Approved)
            {
                campaign.CurrentPlanId = plan.Id;
                campaign.CurrentPercentToBeCharged = plan.PercentToBeCharged;
                campaign.FixedRate = plan.FixedRate;
                campaign.UpdateAt = DateTime.Now;
                await _campaignRepository.Update(campaign);

                body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                            <h2 style='color: #333;'>Upgrade de Alavancagem Aprovado!</h2>
                            <p>Olá {user.Name},</p>
                            <p>Seu pedido de upgrade de alavancagem foi aprovado. A partir de agora, seu plano será:</p>
                            <h3 style='background: #f4f4f4; display: inline-block; padding: 10px 20px; border-radius: 5px;'>
                                {plan.Title}
                            </h3>
                            <p><strong>Descrição:</strong> {plan.Description}</p>
                            <p><strong>Nova Taxa de Alavancagem:</strong> {plan.PercentToBeCharged}% + Taxa Fixa de R$ {Math.Round(plan.FixedRate, 2)}</p>
                            <p>Agradecemos por confiar em nossos serviços!</p>
                            <p style='color: #888; font-size: 12px;'>Este e-mail foi gerado automaticamente. Não responda.</p>
                        </body>
                    </html>
                ";
            }
            else
            {
                body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                            <h2 style='color: #d9534f;'>Upgrade de Alavancagem Reprovado</h2>
                            <p>Olá {user.Name},</p>
                            <p>Infelizmente, seu pedido de upgrade de alavancagem não foi aprovado.</p>
                            <p>Se tiver dúvidas ou quiser mais informações, entre em contato com nosso suporte.</p>
                            <p>Agradecemos pela compreensão.</p>
                            <p style='color: #888; font-size: 12px;'>Este e-mail foi gerado automaticamente. Não responda.</p>
                        </body>
                    </html>
                ";
            }

            leverage.LeverageStatus = leverageStatus;
            leverage.UpdatedAt = DateTime.Now;
            await _leverageRequestRepository.Update(leverage);

            string subject = "Solicitação - Upgrade de Alavancagem";
            await SendMailNotification(subject, body, user.Email);
        }

        public async Task Delete(Guid id)
        {
            var leverageRequest = await _leverageRequestRepository.GetById(id);
            if (leverageRequest == null)
                return;

            foreach (var item in leverageRequest.Files)
                await _s3Service.DeleteFileByFileNameKey(item);

            await _leverageRequestRepository.Delete(id);
        }

        public async Task<PagedResult<LeverageRequest>> GetAll(int page, int size)
        {
            var leverageRequests = await _leverageRequestRepository.GetAll(page, size);

            var filesLink = new List<string>();
            foreach (var leverageRequest in leverageRequests.Items)
            {
                leverageRequest.Campaign = await _campaignRepository.GetById(leverageRequest.CampaignId);
                leverageRequest.Plan = await _planRepository.GetById(leverageRequest.PlanId);

                foreach (var item in leverageRequest.Files)
                {
                    var file = await _s3Service.GetFileUrlByFileNameKey(item);
                    if (!string.IsNullOrEmpty(file))
                        filesLink.Add(file);
                }

                leverageRequest.User = await _userService.GetById(leverageRequest.UserId);
            }

            return leverageRequests;
        }

        public async Task<LeverageRequest> GetByCampaignId(Guid campaignId)
        {
            return await _leverageRequestRepository.GetByCampaignId(campaignId);
        }

        public async Task<List<LeverageRequest>> GetByUserId(Guid userId)
        {
            var leverageRequests = await _leverageRequestRepository.GetByUserId(userId);
            if (leverageRequests.Count == 0)
                return [];

            var filesLink = new List<string>();
            foreach (var leverageRequest in leverageRequests)
            {
                foreach (var item in leverageRequest.Files)
                {
                    var file = await _s3Service.GetFileUrlByFileNameKey(item);
                    if (!string.IsNullOrEmpty(file))
                        filesLink.Add(file);
                }

                leverageRequest.User = await _userService.GetById(leverageRequest.UserId);
            }

            return leverageRequests;
        }

        public async Task Record(Guid campaignId, Guid userId, Guid planId, 
            string phone, bool hasSharedCampaign, string evidenceLinks,
            bool wantsToBoostCampaign, string preferredContactMethod, Dictionary<Stream, string> FilesAndExtensions)
        {
            List<string> fileKeys = [];
            foreach (var file in FilesAndExtensions)
            {
                var filenamekey = await _s3Service.SendStreamFileToS3(file.Key, file.Value);
                if (!string.IsNullOrEmpty(filenamekey))
                    fileKeys.Add(filenamekey);
            }

            LeverageRequest leverageRequest = new(campaignId,
                userId, planId, phone, hasSharedCampaign, evidenceLinks, wantsToBoostCampaign, preferredContactMethod,
                fileKeys);

            await _leverageRequestRepository.Record(leverageRequest);

            var campaign = await campaignRepository.GetById(campaignId);
            var user = await userService.GetById(userId);
            var plan = await _planRepository.GetById(planId);

            string[] receivers = _configuration["Mail:WebMasters"].Split(",");
            string title = $"Alavancagem de Vaquinha - {campaign.Title}";
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            // Dados da campanha
            string campaignTitle = campaign.Title;
            string campaignUrl = $"{_configuration["System:FrontendUrl"]}/vaquinha/{campaign.Slug}";
            string campaignSlug = campaign.Slug;

            // Dados do usuário
            string userName = user.Name;
            string userDocId = user.DocumentId;

            //// Dados do plano escolhido
            string planTitle = plan.Title;
            string planDescription = plan.Description;

            //// Construção do corpo do email em HTML
            string body = $@"
                <html>
                    <body>
                        <h2>Detalhes da Campanha</h2>
                        <p><strong>ID da Campanha:</strong> {campaignId}</p>
                        <p><strong>Título da Campanha:</strong> {campaignTitle}</p>
                        <p><strong>Slug da Campanha:</strong> {campaignSlug}</p>                        
                        <p><strong>URL da Campanha:</strong> <a>{campaignUrl}</a></p>

                        <h2>Detalhes do Usuário</h2>
                        <p><strong>ID do Usuário:</strong> {userId}</p>
                        <p><strong>Nome do Usuário:</strong> {userName}</p>
                        <p><strong>ID do Documento do Usuário:</strong> {userDocId}</p>

                        <h2>Detalhes do Plano Escolhido</h2>
                        <p><strong>ID do Plano:</strong> {planId}</p>
                        <p><strong>Título do Plano:</strong> {planTitle}</p>
                        <p><strong>Descrição do Plano:</strong> {planDescription}</p>
                    </body>
                </html>
            ";

            await _emailService.Send(receivers, title, body, businessAddresses,
                businessName, null);
        }

        protected async Task SendMailNotification(string subject, string body, string receptor)
        {
            string[] receivers = [receptor];
            string title = subject;
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            await _emailService.Send(receivers, title, body, businessAddresses,
                businessName, null);
        }
    }
}

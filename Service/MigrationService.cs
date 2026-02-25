using Domain;
using Domain.Bank;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.Bank;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.System;

namespace Service
{
    public class MigrationService(
        IGroupAccessRepository groupAccessRepository,
        ICategoryRepository categoryRepository,
        IPlanRepository planRepository,
        IGatewayConfigurationRepository gatewayConfigurationRepository,
        IDigitalStickerRepository digitalStickerRepository,
        IBankAccountRepository bankAccountRepository,
        ICampaignRepository campaignRepository,
        IBaasConfigurationRepository baasConfigurationRepository) : IMigrationService
    {
        private readonly IGroupAccessRepository _groupAccessRepository = groupAccessRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IPlanRepository _planRepository = planRepository;
        private readonly IGatewayConfigurationRepository _gatewayConfigurationRepository = gatewayConfigurationRepository;
        private readonly IDigitalStickerRepository _digitalStickerRepository = digitalStickerRepository;
        private readonly IBankAccountRepository _bankAccountRepository = bankAccountRepository;
        private readonly IBaasConfigurationRepository _baasConfigurationRepository = baasConfigurationRepository;

        private readonly ICampaignRepository _campaignRepository = campaignRepository;

        public async Task GroupAccessMigration()
        {
            // Verifica se já existem grupos de acesso
            var existingGroups = await _groupAccessRepository.GetAll();

            if (existingGroups.Count == 0)
            {
                // Se não existirem grupos, cria alguns grupos padrão
                var defaultGroups = new List<GroupAccess>
                {
                    new("Admin", "Acesso total ao sistema"){ Id = new Guid("e090e9fd-13a6-4e3d-a3f1-155217cb9db5") },
                    new("User", "Acesso padrão ao sistema"){ Id = new Guid("b09bb53e-f518-433b-a94f-253386876fcc") },
                    new("Guest", "Acesso restrito") { Id = new Guid("488bf5f9-2c01-4bf6-9ee4-9ea5d34739a2") }
                };

                foreach (var group in defaultGroups)
                {
                    await _groupAccessRepository.Add(group);
                }
            }
        }

        public async Task CategoryMigration()
        {
            var existingCategories = await _categoryRepository.GetAll();

            if (existingCategories.Count == 0)
            {
                var predefinedCategories = new List<Category>
                {
                    new("Saúde", "Categoria para arrecadações relacionadas à saúde") { Id = new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479") },
                    new("Educação", "Categoria para arrecadações relacionadas à educação") { Id = new Guid("a3b59912-14d4-40d3-a244-2b0b34db93f7") },
                    new("Projetos Sociais", "Categoria para projetos de impacto social") { Id = new Guid("7b7a44a2-2db5-4e8b-822b-8b61095d56ed") },
                    new("Eventos e Festas", "Categoria para arrecadações para eventos e festas") { Id = new Guid("e2a713f9-9dd9-4e50-9416-b9d0b76383d9") },
                    new("Empreendedorismo", "Categoria para financiamento de negócios e startups") { Id = new Guid("22ae1672-cf14-4bdf-b1b5-531f57314f2d") },
                    new("Emergências", "Categoria para arrecadações emergenciais") { Id = new Guid("5c926f8f-87b7-485a-abc7-646e3a1c0eaf") },
                    new("Esportes", "Categoria para arrecadações relacionadas a esportes") { Id = new Guid("cfd9fc84-8c7a-489f-9f67-429bf3478c99") },
                    new("Animais", "Categoria para arrecadações relacionadas aos animais") { Id = new Guid("1c6ec7ab-5041-4568-85d7-7e90ecdb5a34") },
                    new("Tecnologia e Inovação", "Categoria para projetos de tecnologia e inovação") { Id = new Guid("9bfc8ad4-5f61-4d75-9c61-8ff5d125c5a6") },
                    new("Cultura e Arte", "Categoria para arrecadações de projetos culturais e artísticos") { Id = new Guid("55a2c9a0-d5b6-49e3-9bb9-034d015c1a0f") },
                    new("Viagens", "Categoria para arrecadações de viagens") { Id = new Guid("0d104d34-8570-4a36-9c37-dcecc46d0d5d") },
                    new("Reformas e Melhorias", "Categoria para reformas de imóveis e infraestrutura") { Id = new Guid("df91d00c-5d3f-4a28-9d74-bb86d7d1c8ec") },
                    new("Comunidade", "Categoria para projetos comunitários") { Id = new Guid("f964a5a7-574f-4de5-9a57-2737a7b7f5d5") }
                };

                foreach (var category in predefinedCategories)
                {
                    await _categoryRepository.Add(category);
                }
            }
        }

        public async Task PlansMigration()
        {
            var plans = await _planRepository.GetAll();

            if(plans.Count == 0)
            {
                string[] benefitsOrange = [ "Amplo Alcance nas Redes Sociais", "Estratégia Personalizada", "Engajamento Ativo",
                "Análise de Desempenho"];

                var kaixinhaOrange = new Plan(
                    "🍊 Plano Orange", 
                    "O Plano Orange é uma solução abrangente projetada para maximizar a visibilidade e engajamento de sua Kaixinha, utilizando estratégias de divulgação adaptadas para as principais plataformas de redes sociais.",
                    benefitsOrange, 
                    Convert.ToDecimal(20.0),
                    Convert.ToDecimal(0), 
                    false, 
                    true, 
                    2);


                string[] benefitsDiamond = [ "Todos os Benefícios do Plano Orange", "Tráfego Pago Totalmente Gerenciado",
                    "Alcance Nacional",
                    "Análise de Resultados"];

                var kaixinhaDiamond = new Plan("💎 Plano Diamond",
                    "O Plano Orange Diamond é uma extensão poderosa do Plano Orange, oferecendo todos os recursos já inclusos, mas com o acréscimo de tráfego pago. Essa adição permite que a divulgação da marca seja potencializada através de campanhas patrocinadas.",
                    benefitsDiamond,
                    Convert.ToDecimal(35.0f),
                    Convert.ToDecimal(0),
                    true, 
                    false, 
                    1);

                string[] benefitsFacil = [""];

                var kaixinhaFacil = new Plan(
                    "Padrão",
                    "Plano padrão",
                    [],
                    Convert.ToDecimal(9.9f),
                    Convert.ToDecimal(0),
                    true,
                    false,
                    0);

                await _planRepository.Add(kaixinhaFacil);
                await _planRepository.Add(kaixinhaOrange);
                await _planRepository.Add(kaixinhaDiamond);
            }
        }

        public async Task GatewayConfiguration()
        {
            var config = await _gatewayConfigurationRepository.Get();
            if (config != null) return;

            GatewayConfiguration gc = new()
            {
                Pix = Gateway.ReflowPay,
                Card = Gateway.ReflowPay,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _gatewayConfigurationRepository.Add(gc);
        }

        public async Task DigitalStickerMigration()
        {
            var digitalSticker = await _digitalStickerRepository.List();
            if (digitalSticker.Count == 0) {
                DigitalSticker digitalSticker1 = new("Uma ótima escolha para começar a destacar sua vaquinha.", 490, 3, 0);
                await Task.Delay(1000);
                DigitalSticker digitalSticker2 = new("Impulsione sua vaquinha e atraia mais apoiadores.", 1299, 15, 0);
                await Task.Delay(1000);
                DigitalSticker digitalSticker3 = new("Destaque sua vaquinha de forma ainda mais especial!", 1499, 15, 500, true);

                await _digitalStickerRepository.Add(digitalSticker1);
                await _digitalStickerRepository.Add(digitalSticker2);
                await _digitalStickerRepository.Add(digitalSticker3);
            }
        }

        public async Task CreateSystemBankAccount()
        {
            var systemBankAccount = await _bankAccountRepository.GetSystemAccount();
            if (systemBankAccount == null) 
            {
                BankAccount account = new(Guid.NewGuid(), "", 0, BankAccountType.Organization);
                await _bankAccountRepository.CreateAsync(account);
            }
        }

        public async Task RenamePropertieInCampaign()
        {
            await _campaignRepository.RenameNewPlanIdToCurrentPlanIdAsync();
            await _campaignRepository.RenameNewPercentToBeChargedToCurrentPercentToBeCharged();
        }

        public async Task CreateBAASConfiguration()
        {
            if(await _baasConfigurationRepository.Get() == null)
            {
                BaasConfiguration baas = new()
                {
                    AnalyseWithdraw = true,
                    DailyWithdrawalLimitValue = 1000000,
                    DailyWithdrawalMinimumValue = 1000
                };

                await _baasConfigurationRepository.Add(baas);
            }
        }
    }
}

using Domain;
using Domain.Bank;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;
using Newtonsoft.Json;
using Util;

namespace Service
{
    public class StoreService : IStoreService
    {
        protected readonly IDigitalStickerRepository _digitalStickerRepository;
        protected readonly IUserDigitalStickersRepository _userDigitalStickersRepository;
        protected readonly IUserDigitalStickersUsageRepository _userDigitalStickersUsageRepository;
        protected readonly IUserService _userService;
        protected readonly IPaymentService _paymentService;
        protected readonly IBankService _bankService;
        protected readonly ICacheService _cacheService;

        public StoreService(IDigitalStickerRepository digitalStickerRepository,
            IUserService userService,
            IPaymentService paymentService,
            IBankService bankService,
            ICacheService cacheService,
            IUserDigitalStickersRepository userDigitalStickersRepository,
            IUserDigitalStickersUsageRepository userDigitalStickersUsageRepository)
        {
            _digitalStickerRepository = digitalStickerRepository;
            _userService = userService;
            _paymentService = paymentService;
            _bankService = bankService;
            _cacheService = cacheService;
            _userDigitalStickersRepository = userDigitalStickersRepository;
            _userDigitalStickersUsageRepository = userDigitalStickersUsageRepository;
        }

        public async Task<TransactionData> AddToCartDigitalStickers(Guid? campaignId, Guid userId, Guid planId, int qtd, string clientIp)
        {
            var plan = await _digitalStickerRepository.GetById(planId);
            var user = await _userService.GetById(userId);

            Domain.Objects.ReflowPay.Item item = new()
            {
                Title = $"{plan.Qtd} Kaixinha",
                Description = plan.Description,
                UnitPrice = plan.Price,
                Quantity = plan.Qtd,
                Tangible = false
            };

            var paymentinfo = await _paymentService.GeneratePix(plan.Price, clientIp, item, user, 
                Strings.Webhooks["digitalStickers"]);

            paymentinfo.TransactionSource = BankTransactionSource.DigitalSticker;

            UserDigitalStickers userDigitalStickers = new(
                campaignId,
                userId,
                paymentinfo.Id.ToString(),
                plan.Qtd,
                plan.Price,
                paymentinfo.Gateway,
                TransationMethod.Cash,
                DonationStatus.Created,
                DonationType.SmallDonations);

            await _userDigitalStickersRepository.Add(userDigitalStickers);

            await _cacheService.Set(paymentinfo.Id, JsonConvert.SerializeObject(paymentinfo), 1);

            return paymentinfo;
        }

        public async Task<bool> CheckPaymentDigitalStickers(string transactionId)
        {
            var transaction = await _userDigitalStickersRepository.GetByTransactionId(transactionId);
            if (transaction == null) return false;

            var payed = await _paymentService.ConfirmPix(transaction.Gateway, transactionId);

            return payed;
        }

        public async Task ConfirmPaymentDigitalStickers(string transactionId)
        {
            var userDigitalStickers = await _userDigitalStickersRepository.GetByTransactionId(transactionId);
            if(userDigitalStickers != null)
            {
                userDigitalStickers.Status = DonationStatus.Paid;
                userDigitalStickers.UpdatedAt = DateTime.Now;

                await _userDigitalStickersRepository.Update(userDigitalStickers);

                await _paymentService.ConfirmTransaction(userDigitalStickers.Gateway, transactionId);

                var systemBankAccount = await _bankService.GetSystemAccount();

                decimal total = (decimal)userDigitalStickers.Value / 100;

                BankTransaction transactionSystem = new(systemBankAccount.Id, null, 0, 0, total,
                BankTransactionType.CashIn, BankTransactionStatus.Completed, BankTransactionSource.DigitalSticker, $"Recebimento de Kaixinha", null, DateTime.Now);
                await _bankService.AddTransaction(transactionSystem);

                // Se a campanha foi informada então ele já atribui a quantidade a campanha
                if (userDigitalStickers.CampaignId != null)
                {
                    await AddUsageDigitalStickers(userDigitalStickers.UserId, 
                        (Guid)userDigitalStickers.CampaignId, userDigitalStickers.Quantity);
                }
            }
        }

        public async Task<List<DigitalSticker>> GetAllDigitalStickers()
        {
            return await _digitalStickerRepository.List();
        }

        public async Task<List<UserDigitalStickers>> GetUserDigitalStickers(Guid userId)
        {
            var stickers = await _userDigitalStickersRepository.GetByUserId(userId);
            return stickers;
        }

        public async Task AddUsageDigitalStickers(Guid userId, Guid campaignId, int amount)
        {
            var stickers = await GetUserDigitalStickers(userId);
            var usages = await GetUserDigitalStickersUsage(userId);

            int totalStickers = stickers.Where(x => x.Status == DonationStatus.Paid).ToList()
                .Sum(sticker => sticker.Quantity);
            int totalUsed = usages.Sum(usage => usage.Quantity);

            int totalAvailable = totalStickers - totalUsed;

            // Verifique se há stickers suficientes disponíveis
            if (totalAvailable < amount)
                throw new InvalidOperationException($"Usuário possui apenas {totalAvailable} stickers disponíveis, mas tentou usar {amount}.");

            // Adicione o uso de stickers
            var stickersUsage = new UserDigitalStickersUsage(userId, campaignId, amount);
            await _userDigitalStickersUsageRepository.Add(stickersUsage);
        }

        public async Task<List<UserDigitalStickersUsage>> GetUserDigitalStickersUsage(Guid userId)
        {
            var usage = await _userDigitalStickersUsageRepository.GetByUserId(userId);
            return usage;
        }

        public async Task<List<UserDigitalStickersUsage>> GetDigitalStickersByCampaignId(Guid campaignId)
        {
            var digitalStickers = await _userDigitalStickersUsageRepository.ListByCampaignId(campaignId);

            return digitalStickers;
        }
    }
}

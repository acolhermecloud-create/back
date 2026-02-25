using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface IStoreService
    {
        Task<List<DigitalSticker>> GetAllDigitalStickers();

        Task<List<UserDigitalStickers>> GetUserDigitalStickers(Guid userId);

        Task<TransactionData> AddToCartDigitalStickers(Guid? campaignId, Guid userId, Guid planId, int qtd, string clientIp);

        Task<bool> CheckPaymentDigitalStickers(string transactionId);

        Task ConfirmPaymentDigitalStickers(string transactionId);

        Task<List<UserDigitalStickersUsage>> GetUserDigitalStickersUsage(Guid userId);

        Task<List<UserDigitalStickersUsage>> GetDigitalStickersByCampaignId(Guid campaignId);

        Task AddUsageDigitalStickers(Guid userId, Guid campaignId, int amount);
    }
}

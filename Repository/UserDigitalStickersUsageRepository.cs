using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserDigitalStickersUsageRepository : IUserDigitalStickersUsageRepository
    {
        private readonly IMongoCollection<UserDigitalStickersUsage> _collection;

        public UserDigitalStickersUsageRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserDigitalStickersUsage>(UserDigitalStickersUsage.TABLE_NAME);
        }

        public async Task Add(UserDigitalStickersUsage userDigitalStickersUsage)
        {
            await _collection.InsertOneAsync(userDigitalStickersUsage);
        }

        public async Task<List<UserDigitalStickersUsage>> GetByUserId(Guid userId)
        {
            var filter = Builders<UserDigitalStickersUsage>.Filter.Eq(usage => usage.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<UserDigitalStickersUsage>> ListByCampaignId(Guid campaignId)
        {
            var filter = Builders<UserDigitalStickersUsage>.Filter.Eq(usage => usage.CampaignId, campaignId);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}

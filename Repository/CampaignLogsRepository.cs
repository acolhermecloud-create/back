using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CampaignLogsRepository : ICampaignLogsRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<CampaignLogs> _collection;

        public CampaignLogsRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CampaignLogs>(CampaignLogs.TABLE_NAME);
        }

        public async Task Add(CampaignLogs campaignLog)
        {
            await _collection.InsertOneAsync(campaignLog);
        }

        public async Task<List<CampaignLogs>> GetByCampaignId(Guid campaignId)
        {
            var filter = Builders<CampaignLogs>.Filter.Eq(log => log.CampaignId, campaignId);
            return await _collection.Find(filter).SortByDescending(x => x.CreateAt).ToListAsync();
        }
    }
}

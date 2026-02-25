using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CampaignReportRepository : ICampaignReportRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<CampaignReport> _collection;

        public CampaignReportRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CampaignReport>(CampaignReport.TABLE_NAME);
        }

        public async Task Add(CampaignReport campaignReport)
        {
            campaignReport.CreateAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(campaignReport);
        }

        public async Task<List<CampaignReport>> GetByCampaignIdAsync(Guid campaignId)
        {
            var filter = Builders<CampaignReport>.Filter.Eq(report => report.CampaignId, campaignId);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}

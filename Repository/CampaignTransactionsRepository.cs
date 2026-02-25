using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CampaignTransactionsRepository : ICampaignTransactionsRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<CampaignTransactions> _collection;

        public CampaignTransactionsRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CampaignTransactions>(CampaignTransactions.TABLE_NAME);
        }

        public async Task AddTransactionAsync(CampaignTransactions transaction)
        {
            await _collection.InsertOneAsync(transaction);
        }

        public async Task DeleteTransactionAsync(Guid id)
        {
            var filter = Builders<CampaignTransactions>.Filter.Eq(t => t.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<CampaignTransactions> GetTransactionByIdAsync(Guid id)
        {
            var filter = Builders<CampaignTransactions>.Filter.Eq(t => t.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CampaignTransactions>> GetTransactionsByCampaignIdAsync(Guid campaignId)
        {
            var filter = Builders<CampaignTransactions>.Filter.Eq(t => t.CampaignId, campaignId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<CampaignTransactions>> GetTransactionsByDonorIdAsync(Guid donorId)
        {
            var filter = Builders<CampaignTransactions>.Filter.Eq(t => t.DonorId, donorId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task UpdateTransactionAsync(CampaignTransactions transaction)
        {
            var filter = Builders<CampaignTransactions>.Filter.Eq(t => t.Id, transaction.Id);
            await _collection.ReplaceOneAsync(filter, transaction);
        }
    }
}

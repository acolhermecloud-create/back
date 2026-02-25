using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CampaignCommentsRepository : ICampaignCommentsRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<CampaignComments> _collection;

        public CampaignCommentsRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CampaignComments>(CampaignComments.TABLE_NAME);
        }

        public async Task Add(CampaignComments campaignComment)
        {
            await _collection.InsertOneAsync(campaignComment);
        }

        public async Task<List<CampaignComments>> ListByCampaignId(Guid campaignId)
        {
            var filter = Builders<CampaignComments>.Filter.Eq(comment => comment.CampaignId, campaignId);
            return await _collection.Find(filter).SortByDescending(x => x.CreateAt).ToListAsync();
        }

        public async Task RemoveById(Guid commentId)
        {
            var filter = Builders<CampaignComments>.Filter.Eq(comment => comment.Id, commentId);
            await _collection.DeleteOneAsync(filter);
        }
    }
}

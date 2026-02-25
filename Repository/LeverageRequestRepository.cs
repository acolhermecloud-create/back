using Domain;
using Domain.Interfaces.Repository;
using Domain.Objects;
using MongoDB.Driver;

namespace Repository
{
    public class LeverageRequestRepository : ILeverageRequestRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<LeverageRequest> _collection;

        public LeverageRequestRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<LeverageRequest>(LeverageRequest.TABLE_NAME);
        }

        public async Task Delete(Guid id)
        {
            var filter = Builders<LeverageRequest>.Filter.Eq(lr => lr.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<PagedResult<LeverageRequest>> GetAll(int page, int pageSize)
        {
            // Obter o total de itens que correspondem ao filtro
            var totalItems = await _collection.CountDocumentsAsync(Builders<LeverageRequest>.Filter.Empty);

            var items = await _collection.Find(_ => true)
                .SortBy(x => x.LeverageStatus)
                .Skip((page - 1) * pageSize)
                .ToListAsync();

            // Calcular se há mais itens
            var hasMoreItems = (page * pageSize) < totalItems;

            return new PagedResult<LeverageRequest>
            {
                Items = items,
                TotalItems = totalItems,
                HasMoreItems = hasMoreItems
            };
        }

        public async Task<LeverageRequest> GetByCampaignId(Guid campaignId)
        {
            return await _collection.Find(x => x.CampaignId == campaignId).FirstOrDefaultAsync();
        }

        public async Task<LeverageRequest> GetById(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<LeverageRequest>> GetByUserId(Guid userId)
        {
            var filter = Builders<LeverageRequest>.Filter.Eq(lr => lr.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task Record(LeverageRequest leverageRequest)
        {
            await _collection.InsertOneAsync(leverageRequest);
        }

        public async Task Update(LeverageRequest leverageRequest)
        {
            var filter = Builders<LeverageRequest>.Filter.Eq(lr => lr.Id, leverageRequest.Id);
            await _collection.ReplaceOneAsync(filter, leverageRequest);
        }
    }
}

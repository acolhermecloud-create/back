using Domain;
using Domain.Interfaces.Repository;
using Domain.Objects;
using MongoDB.Driver;

namespace Repository
{
    public class PlanRepository : IPlanRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Plan> _collection;

        public PlanRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Plan>(Plan.TABLE_NAME);
        }

        public async Task Add(Plan plan)
        {
            await _collection.InsertOneAsync(plan);
        }

        public async Task Update(Plan plan)
        {
            var filter = Builders<Plan>.Filter.Eq(p => p.Id, plan.Id);
            await _collection.ReplaceOneAsync(filter, plan);
        }

        public async Task Desactive(Guid planId)
        {
            var filter = Builders<Plan>.Filter.Eq(p => p.Id, planId);
            var update = Builders<Plan>.Update.Set(p => p.Active, false);
            await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<List<Plan>> GetAll()
        {
            return await _collection.Find(_ => true).SortByDescending(x => x.Position).ToListAsync();
        }

        public async Task<Plan> GetDefault()
        {
            return await _collection.Find(p => p.Default == true).FirstOrDefaultAsync();
        }

        public async Task<Plan> GetById(Guid planId)
        {
            return await _collection.Find(x => x.Id == planId).FirstOrDefaultAsync();
        }

        public async Task<PagedResult<Plan>> GetAll(int page, int pageSize)
        {
            var filter = Builders<Plan>.Filter.Empty;
            var totalItems = await _collection.CountDocumentsAsync(filter);

            // Obter a lista de itens paginados
            var items = await _collection.Find(filter)
                .SortByDescending(doc => doc.Position)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Calcular se há mais itens
            var hasMoreItems = (page * pageSize) < totalItems;

            return new PagedResult<Plan>
            {
                Items = items,
                TotalItems = totalItems,
                HasMoreItems = hasMoreItems
            };
        }
    }
}

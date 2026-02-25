using Domain.Acquirers;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class TransfeeraRepository : ITransfeeraRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Transfeera> _collection;

        public TransfeeraRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Transfeera>(Transfeera.TABLE_NAME);
        }

        public async Task AddOrUpdate(Transfeera transfeera)
        {
            if(transfeera.Id == Guid.Empty)
                await _collection.InsertOneAsync(transfeera);
            else
                await Update(transfeera);
        }

        public async Task<Transfeera> Get() => await _collection.Find(Builders<Transfeera>.Filter.Empty).FirstOrDefaultAsync();

        protected async Task Update(Transfeera plan)
        {
            var filter = Builders<Transfeera>.Filter.Eq(p => p.Id, plan.Id);
            await _collection.ReplaceOneAsync(filter, plan);
        }
    }
}

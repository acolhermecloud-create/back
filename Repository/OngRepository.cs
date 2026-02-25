using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class OngRepository : IOngRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Ong> _collection;

        public OngRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Ong>(Ong.TABLE_NAME);
        }

        public async Task Add(Ong ong)
        {
            await _collection.InsertOneAsync(ong);
        }

        public async Task Update(Ong ong)
        {
            var filter = Builders<Ong>.Filter.Eq(o => o.Id, ong.Id);
            await _collection.ReplaceOneAsync(filter, ong);
        }

        public async Task Delete(Guid id)
        {
            var filter = Builders<Ong>.Filter.Eq(o => o.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<Ong> GetById(Guid id)
        {
            var filter = Builders<Ong>.Filter.Eq(o => o.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Ong>> GetAll(int page, int pageSize)
        {
            return await _collection.Find(_ => true)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}
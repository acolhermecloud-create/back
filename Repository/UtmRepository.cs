using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UtmRepository : IUtmRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Utm> _collection;

        public UtmRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Utm>(Utm.TABLE_NAME);
        }
        public async Task Add(Utm utm) => await _collection.InsertOneAsync(utm);

        public async Task<Utm> GetByOrderId(string orderId)
            => await _collection.Find(x => x.OrderId == orderId).FirstOrDefaultAsync();

        public async Task Update(Utm utm)
        {
            var filter = Builders<Utm>.Filter.Eq(uds => uds.OrderId, utm.OrderId);
            await _collection.ReplaceOneAsync(filter, utm);
        }
    }
}

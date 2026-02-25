using Domain.Acquirers;
using Domain.Interfaces.Repository.Acquirers;
using MongoDB.Driver;

namespace Repository.Acquirers
{
    public class ReflowPayRepository : IReflowPayRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<ReflowPay> _collection;

        public ReflowPayRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<ReflowPay>(ReflowPay.TABLE_NAME);
        }

        public async Task<ReflowPay> GetFees()
            => await _collection.Find(Builders<ReflowPay>.Filter.Empty).FirstOrDefaultAsync();

        public async Task Add(ReflowPay reflowPay) => await _collection.InsertOneAsync(reflowPay);

        public async Task Update(ReflowPay reflowPay)
        {
            var filter = Builders<ReflowPay>.Filter.Eq(c => c.Id, reflowPay.Id);
            await _collection.ReplaceOneAsync(filter, reflowPay);
        }
    }
}

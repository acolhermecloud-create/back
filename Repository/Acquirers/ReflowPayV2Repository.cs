using Domain.Acquirers;
using Domain.Interfaces.Repository.Acquirers;
using MongoDB.Driver;

namespace Repository.Acquirers
{
    public class ReflowPayV2Repository : IReflowPayV2Repository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<ReflowPayV2> _collection;

        public ReflowPayV2Repository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<ReflowPayV2>(ReflowPayV2.TABLE_NAME);
        }

        public async Task<ReflowPayV2> GetFees()
            => await _collection.Find(Builders<ReflowPayV2>.Filter.Empty).FirstOrDefaultAsync();

        public async Task Add(ReflowPayV2 reflowPay) => await _collection.InsertOneAsync(reflowPay);

        public async Task Update(ReflowPayV2 reflowPay)
        {
            var filter = Builders<ReflowPayV2>.Filter.Eq(c => c.Id, reflowPay.Id);
            await _collection.ReplaceOneAsync(filter, reflowPay);
        }
    }
}

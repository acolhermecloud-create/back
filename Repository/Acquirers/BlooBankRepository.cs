using Domain.Acquirers;
using Domain.Interfaces.Repository.Acquirers;
using MongoDB.Driver;

namespace Repository.Acquirers
{
    public class BlooBankRepository : IBlooBankRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<BlooBank> _collection;

        public BlooBankRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<BlooBank>(BlooBank.TABLE_NAME);

        }

        public async Task Add(BlooBank bloobank) => await _collection.InsertOneAsync(bloobank);

        public async Task<BlooBank> GetFees()
            => await _collection.Find(Builders<BlooBank>.Filter.Empty).FirstOrDefaultAsync();

        public async Task UpdateFees(BlooBank bloobank)
        {
            var filter = Builders<BlooBank>.Filter.Eq(c => c.Id, bloobank.Id);
            await _collection.ReplaceOneAsync(filter, bloobank);
        }
    }
}

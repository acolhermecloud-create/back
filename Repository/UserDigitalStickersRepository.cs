using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserDigitalStickersRepository : IUserDigitalStickersRepository
    {
        private readonly IMongoCollection<UserDigitalStickers> _collection;

        public UserDigitalStickersRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserDigitalStickers>(UserDigitalStickers.TABLE_NAME);
        }

        public async Task Add(UserDigitalStickers userDigitalStickers)
        {
            await _collection.InsertOneAsync(userDigitalStickers);
        }

        public async Task Update(UserDigitalStickers userDigitalStickers)
        {
            var filter = Builders<UserDigitalStickers>.Filter.Eq(uds => uds.Id, userDigitalStickers.Id);
            await _collection.ReplaceOneAsync(filter, userDigitalStickers);
        }

        public async Task<List<UserDigitalStickers>> GetByUserId(Guid userId)
        {
            var filter = Builders<UserDigitalStickers>.Filter.Eq(uds => uds.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<UserDigitalStickers> GetByTransactionId(string transactionId)
        {
            var filter = Builders<UserDigitalStickers>.Filter.Eq(uds => uds.TransactionId, transactionId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<UserDigitalStickers> GetByIdAsync(Guid id)
        {
            var filter = Builders<UserDigitalStickers>.Filter.Eq(uds => uds.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}

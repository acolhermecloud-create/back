using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserPaymentAccountRepository : IUserPaymentAccountRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserPaymentAccount> _collection;

        public UserPaymentAccountRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserPaymentAccount>(UserPaymentAccount.TABLE_NAME);
        }

        public async Task Add(UserPaymentAccount account)
        {
            await _collection.InsertOneAsync(account);
        }

        public async Task Update(UserPaymentAccount account)
        {
            var filter = Builders<UserPaymentAccount>.Filter.Eq(a => a.Id, account.Id);
            await _collection.ReplaceOneAsync(filter, account);
        }

        public async Task<List<UserPaymentAccount>> GetByUserId(Guid userId)
        {
            var filter = Builders<UserPaymentAccount>.Filter.Eq(a => a.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<UserPaymentAccount?> GetByAccountId(string accountId)
        {
            var filter = Builders<UserPaymentAccount>.Filter.Eq(a => a.AccountId, accountId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task DeactivateAccount(Guid accountId)
        {
            var filter = Builders<UserPaymentAccount>.Filter.Eq(a => a.Id, accountId);
            var update = Builders<UserPaymentAccount>.Update.Set(a => a.Active, false);
            await _collection.UpdateOneAsync(filter, update);
        }
    }
}

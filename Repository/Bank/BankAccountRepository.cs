using Domain.Bank;
using Domain.Interfaces.Repository.Bank;
using MongoDB.Driver;
using System.Transactions;

namespace Repository.Bank
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly IMongoCollection<BankAccount> _collection;

        public BankAccountRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<BankAccount>(BankAccount.TABLE_NAME);
        }

        public async Task<BankAccount?> GetById(Guid id)
        {
            return await _collection.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<BankAccount> GetByUserId(Guid userId)
        {
            return await _collection.Find(a => a.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(BankAccount BankAccount)
        {
            await _collection.InsertOneAsync(BankAccount);
        }

        public async Task Update(BankAccount BankAccount)
        {
            await _collection.ReplaceOneAsync(a => a.Id == BankAccount.Id, BankAccount);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _collection.DeleteOneAsync(a => a.Id == id);
        }

        public async Task<BankAccount> GetSystemAccount()
        {
            return await _collection.Find(x => x.BankAccountType == Domain.BankAccountType.Organization).FirstOrDefaultAsync();
        }

        public async Task UpdatePixKey(Guid accountId, string pixKey)
        {
            var update = Builders<BankAccount>.Update
                .Set(t => t.PixKey, pixKey);

            await _collection.UpdateOneAsync(t => t.Id == accountId, update);
        }
    }
}

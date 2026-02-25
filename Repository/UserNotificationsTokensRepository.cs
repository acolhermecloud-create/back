using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserNotificationsTokensRepository : IUserNotificationsTokenRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserNotificationsTokens> _collection;

        public UserNotificationsTokensRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserNotificationsTokens>(UserNotificationsTokens.TABLE_NAME);
        }

        public async Task Add(UserNotificationsTokens userNotification)
        {
            await _collection.InsertOneAsync(userNotification);
        }

        public async Task<UserNotificationsTokens> GetByUserId(Guid userId)
        {
            var filter = Builders<UserNotificationsTokens>.Filter.Eq(un => un.UserId, userId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<UserNotificationsTokens>> GetAll()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<UserNotificationsTokens>> GetByIds(List<Guid> ids)
        {
            var filter = Builders<UserNotificationsTokens>.Filter.In(un => un.UserId, ids);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}

using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserNotificationsRepository : IUserNotificationsRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserNotifications> _collection;

        public UserNotificationsRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserNotifications>(UserNotifications.TABLE_NAME);
        }

        public async Task Add(UserNotifications notification)
        {
            await _collection.InsertOneAsync(notification);
        }

        public async Task<UserNotifications> GetById(Guid id)
        {
            var filter = Builders<UserNotifications>.Filter.Eq(n => n.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<UserNotifications>> GetByUserId(Guid userId)
        {
            var filter = Builders<UserNotifications>.Filter.Eq(n => n.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task Update(UserNotifications notification)
        {
            var filter = Builders<UserNotifications>.Filter.Eq(n => n.Id, notification.Id);
            await _collection.ReplaceOneAsync(filter, notification);
        }
    }

}

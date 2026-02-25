using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserPointsRepository : IUserPointsRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserPoints> _collection;

        public UserPointsRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserPoints>(UserPoints.TABLE_NAME);
        }

        public async Task Add(UserPoints userPoints)
        {
            await _collection.InsertOneAsync(userPoints);
        }

        public async Task Update(UserPoints userPoints)
        {
            var filter = Builders<UserPoints>.Filter.Eq(up => up.Id, userPoints.Id);
            await _collection.ReplaceOneAsync(filter, userPoints);
        }

        public async Task<List<UserPoints>> GetMoreThan(int page, int pageSize, int points)
        {
            var filter = Builders<UserPoints>.Filter.Gt(up => up.CurrentPoints, points);
            return await _collection.Find(filter).Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<UserPoints> GetByUserId(Guid userId)
        {
            var filter = Builders<UserPoints>.Filter.Eq(up => up.UserId, userId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}

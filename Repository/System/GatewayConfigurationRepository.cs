using Domain.Interfaces.Repository;
using Domain.System;
using MongoDB.Driver;

namespace Repository.System
{
    public class GatewayConfigurationRepository : IGatewayConfigurationRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<GatewayConfiguration> _collection;

        public GatewayConfigurationRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<GatewayConfiguration>(GatewayConfiguration.TABLE_NAME);
        }

        public async Task<GatewayConfiguration> Get()
        {
            var filter = Builders<GatewayConfiguration>.Filter.Empty;
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<GatewayConfiguration> GetByIdAsync(Guid id)
        {
            var filter = Builders<GatewayConfiguration>.Filter.Eq(c => c.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task Add(GatewayConfiguration configuration)
        {
            await _collection.InsertOneAsync(configuration);
        }

        public async Task Update(GatewayConfiguration configuration)
        {
            var filter = Builders<GatewayConfiguration>.Filter.Eq(c => c.Id, configuration.Id);
            await _collection.ReplaceOneAsync(filter, configuration);
        }

        public async Task DeleteAsync(Guid id)
        {
            var filter = Builders<GatewayConfiguration>.Filter.Eq(c => c.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
    }
}

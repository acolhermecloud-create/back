using Domain.Interfaces.Repository.System;
using Domain.System;
using MongoDB.Driver;

namespace Repository.System
{
    public class BaasConfigurationRepository : IBaasConfigurationRepository
    {
        private readonly IMongoCollection<BaasConfiguration> _collection;

        public BaasConfigurationRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<BaasConfiguration>(BaasConfiguration.TABLE_NAME);
        }

        public async Task Add(BaasConfiguration config)
        {
            var filter = Builders<BaasConfiguration>.Filter.Eq(c => c.Id, config.Id);
            var existingConfig = await _collection.Find(filter).FirstOrDefaultAsync();

            if (existingConfig == null)
            {
                // Se não existir, adiciona o novo documento
                await _collection.InsertOneAsync(config);
            }
            else
            {
                // Se já existir, atualiza
                await _collection.ReplaceOneAsync(filter, config);
            }
        }

        public async Task<BaasConfiguration> Get()
        {
            // Como só existe um registro, podemos usar Find e pegar o primeiro (único) documento
            return await _collection.Find(FilterDefinition<BaasConfiguration>.Empty).FirstOrDefaultAsync();
        }
    }
}

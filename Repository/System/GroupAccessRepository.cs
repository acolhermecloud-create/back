using Domain.Interfaces.Repository.System;
using Domain.System;
using MongoDB.Driver;

namespace Repository.System
{
    public class GroupAccessRepository : IGroupAccessRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<GroupAccess> _collection;

        // Construtor que recebe o cliente MongoDB e o nome do banco de dados
        public GroupAccessRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<GroupAccess>(GroupAccess.TABLE_NAME);
        }

        // Implementação do método para adicionar um novo grupo de acesso
        public async Task Add(GroupAccess groupAccess)
        {
            await _collection.InsertOneAsync(groupAccess);
        }

        // Implementação do método para obter um grupo de acesso pelo ID
        public async Task<GroupAccess> GetById(Guid id)
        {
            var filter = Builders<GroupAccess>.Filter.Eq(g => g.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Implementação do método para obter todos os grupos de acesso
        public async Task<List<GroupAccess>> GetAll()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<GroupAccess> GetByName(string name)
        {
            var filter = Builders<GroupAccess>.Filter.Eq(g => g.Name, name);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
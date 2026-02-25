using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class UserGroupAccessRepository : IUserGroupAccessRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserGroupAccess> _collection;

        // Construtor que recebe o cliente MongoDB e o nome do banco de dados
        public UserGroupAccessRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<UserGroupAccess>(UserGroupAccess.TABLE_NAME);
        }

        // Implementação do método para adicionar uma nova associação
        public async Task Add(UserGroupAccess userGroupAccess)
        {
            await _collection.InsertOneAsync(userGroupAccess);
        }

        // Implementação do método para remover uma associação pelo ID
        public async Task Delete(Guid id)
        {
            var filter = Builders<UserGroupAccess>.Filter.Eq(u => u.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        // Implementação do método para obter uma associação pelo ID
        public async Task<UserGroupAccess> GetById(Guid id)
        {
            var filter = Builders<UserGroupAccess>.Filter.Eq(u => u.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Implementação do método para obter associações de um usuário específico
        public async Task<List<UserGroupAccess>> GetByUserId(Guid userId)
        {
            var filter = Builders<UserGroupAccess>.Filter.Eq(u => u.UserId, userId);
            return await _collection.Find(filter).ToListAsync();
        }

        // Implementação do método para obter associações de um grupo de acesso específico
        public async Task<List<UserGroupAccess>> GetByGroupAccessId(Guid groupAccessId)
        {
            var filter = Builders<UserGroupAccess>.Filter.Eq(u => u.GroupAccessId, groupAccessId);
            return await _collection.Find(filter).ToListAsync();
        }

        // Implementação do método para verificar se uma associação já existe
        public async Task<bool> Exists(Guid userId, Guid groupAccessId)
        {
            var filter = Builders<UserGroupAccess>.Filter.And(
                Builders<UserGroupAccess>.Filter.Eq(u => u.UserId, userId),
                Builders<UserGroupAccess>.Filter.Eq(u => u.GroupAccessId, groupAccessId)
            );
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task DeleteByUserId(Guid id)
        {
            var filter = Builders<UserGroupAccess>.Filter.Eq(u => u.UserId, id);
            await _collection.DeleteManyAsync(filter);
        }
    }
}

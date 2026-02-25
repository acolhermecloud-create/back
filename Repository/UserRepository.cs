using Domain;
using Domain.Interfaces.Repository;
using Domain.Objects;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<User> _collection;

        // Construtor que recebe o cliente MongoDB e o nome do banco de dados
        public UserRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<User>(User.TABLE_NAME);
        }

        // Implementação do método para adicionar um novo usuário
        public async Task Add(User user)
        {
            await _collection.InsertOneAsync(user);
        }

        // Implementação do método para atualizar um usuário existente
        public async Task Update(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _collection.ReplaceOneAsync(filter, user);
        }

        // Implementação do método para remover um usuário por ID
        public async Task Delete(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        // Implementação do método para obter um usuário por ID
        public async Task<User> GetById(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Implementação do método para obter todos os usuários
        public async Task<PagedResult<User>> GetWithFilter(string? name, UserType userType, int page, int pageSize)
        {
            var filterBuilder = Builders<User>.Filter;
            var filters = new List<FilterDefinition<User>>
            {
                Builders<User>.Filter.Eq(u => u.Type, userType),
                Builders<User>.Filter.Eq(u => u.Mock, false),
            };

            if (!string.IsNullOrEmpty(name))
            {
                var nameFilter = filterBuilder.Regex(c => c.Name, new BsonRegularExpression(name, "i"));
                var emailFilter = filterBuilder.Regex(c => c.Email, new BsonRegularExpression(name, "i"));
                var phoneFilter = filterBuilder.Regex(c => c.Phone, new BsonRegularExpression(name, "i"));

                filters.Add(filterBuilder.Or(nameFilter, emailFilter, phoneFilter));
            }

            if (filters.Count == 0)
                filters.Add(filterBuilder.Empty);

            var combinedFilter = filterBuilder.And(filters);

            // Obter o total de itens que correspondem ao filtro
            var totalItems = await _collection.CountDocumentsAsync(combinedFilter);

            // Obter a lista de itens paginados
            var items = await _collection.Find(combinedFilter)
                .SortByDescending(doc => doc.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Calcular se há mais itens
            var hasMoreItems = (page * pageSize) < totalItems;

            return new PagedResult<User>
            {
                Items = items,
                TotalItems = totalItems,
                HasMoreItems = hasMoreItems
            };
        }

        // Implementação do método para verificar se o email já existe
        public async Task<bool> EmailExists(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<User> GetByEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<User>> SearchByName(string name)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Regex(u => u.Name, new BsonRegularExpression(name, "i")),
                Builders<User>.Filter.Eq(u => u.Mock, false)
            );
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<long> TotalUsers(UserType userType)
        {
            long totalItems = await _collection.CountDocumentsAsync(
                Builders<User>.Filter.And(
                    Builders<User>.Filter.Eq(x => x.Mock, false),
                    Builders<User>.Filter.Eq(x => x.Type, userType))
                );
            return totalItems;
        }

        public async Task<User> GetByDocumentId(string documentId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.DocumentId, documentId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}

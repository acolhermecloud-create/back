using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Category> _collection;

        public CategoryRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Category>(Category.TABLE_NAME);
        }

        // Adiciona uma nova categoria
        public async Task Add(Category category) => await _collection.InsertOneAsync(category);

        // Atualiza uma categoria existente
        public async Task Update(Category category)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, category.Id);
            await _collection.ReplaceOneAsync(filter, category);
        }

        // Remove uma categoria pelo ID
        public async Task Delete(Guid id)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        // Busca uma categoria pelo ID
        public async Task<Category> GetById(Guid id)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        // Retorna todas as categorias
        public async Task<List<Category>> GetAll()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
    }
}

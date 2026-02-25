using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class DigitalStickerRepository : IDigitalStickerRepository
    {
        private readonly IMongoCollection<DigitalSticker> _collection;

        public DigitalStickerRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<DigitalSticker>(DigitalSticker.TABLE_NAME);
        }

        public async Task<DigitalSticker> GetById(Guid id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task Add(DigitalSticker digitalSticker)
        {
            await _collection.InsertOneAsync(digitalSticker);
        }

        public async Task<List<DigitalSticker>> List()
        {
            return await _collection.Find(Builders<DigitalSticker>.Filter.Empty).SortByDescending(x => x.CreatedAt).ToListAsync();
        }
    }
}
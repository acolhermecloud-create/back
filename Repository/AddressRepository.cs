using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Address> _collection;

        public AddressRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Address>(Address.TABLE_NAME);
        }

        public async Task Add(Address address) => await _collection.InsertOneAsync(address);

        public async Task Update(Address address)
        {
            var filter = Builders<Address>.Filter.Eq(a => a.Id, address.Id);
            await _collection.ReplaceOneAsync(filter, address);
        }

        public async Task Delete(Guid id)
        {
            var filter = Builders<Address>.Filter.Eq(a => a.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<Address> GetById(Guid id)
        {
            var filter = Builders<Address>.Filter.Eq(a => a.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}

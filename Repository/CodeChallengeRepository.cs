using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class CodeChallengeRepository : ICodeChallengeRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<CodeChallenge> _collection;

        public CodeChallengeRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<CodeChallenge>(CodeChallenge.TABLE_NAME);
        }

        public async Task Add(CodeChallenge codeChallenge)
        {
            await _collection.InsertOneAsync(codeChallenge);
        }

        public async Task Update(CodeChallenge codeChallenge)
        {
            var filter = Builders<CodeChallenge>.Filter.Eq(c => c.Id, codeChallenge.Id);
            await _collection.ReplaceOneAsync(filter, codeChallenge);
        }

        public async Task<CodeChallenge?> GetByReferenceAndCodeValid(string referenceId, string code)
        {
            var filter = Builders<CodeChallenge>.Filter.And(
                Builders<CodeChallenge>.Filter.Eq(c => c.Reference, referenceId),
                Builders<CodeChallenge>.Filter.Eq(c => c.Code, code),
                Builders<CodeChallenge>.Filter.Gte(c => c.ValidAt, DateTime.Now)
            );

            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}

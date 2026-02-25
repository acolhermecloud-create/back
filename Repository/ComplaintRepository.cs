using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Driver;

namespace Repository
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly IMongoCollection<Complaint> _complaintCollection;

        public ComplaintRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _complaintCollection = database.GetCollection<Complaint>(Complaint.TABLE_NAME);
        }

        public async Task Add(Complaint complaint)
        {
            await _complaintCollection.InsertOneAsync(complaint);
        }

        public async Task RemoveById(Guid complaintId)
        {
            var filter = Builders<Complaint>.Filter.Eq(c => c.Id, complaintId);
            await _complaintCollection.DeleteOneAsync(filter);
        }

        public async Task Update(Complaint complaint)
        {
            var filter = Builders<Complaint>.Filter.Eq(c => c.Id, complaint.Id);
            await _complaintCollection.ReplaceOneAsync(filter, complaint);
        }

        public async Task<List<Complaint>> Get(int page, int pageSize)
        {
            return await _complaintCollection.Find(_ => true)
                                             .Skip((page - 1) * pageSize)
                                             .Limit(pageSize)
                                             .ToListAsync();
        }

        public async Task<Complaint> GetById(Guid complaintId)
            => await _complaintCollection.Find(x => x.Id == complaintId).FirstOrDefaultAsync();
    }
}
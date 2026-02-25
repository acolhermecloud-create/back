using Domain;
using Domain.Interfaces.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Repository
{
    public class DonationRepository : ICampaignDonationRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Donation> _collection;
        private readonly IMongoCollection<User> _collectionUsers;

        public DonationRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Donation>(Donation.TABLE_NAME);
            _collectionUsers = database.GetCollection<User>(User.TABLE_NAME);
        }

        public async Task Add(Donation donation)
        {
            await _collection.InsertOneAsync(donation);
        }

        public async Task<Donation> GetById(Guid id)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Donation>> GetByCampaignId(Guid campaignId, int page, int pageSize)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId);
            return await _collection.Find(filter).SortByDescending(x => x.DonatedAt)
                .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task<List<Donation>> GetByDonorId(Guid donorId, int page, int pageSize)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.DonorId, donorId);

            return await _collection.Find(filter)
                .SortByDescending(x => x.DonatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Donation>> GetAll(int page, int pageSize)
        {
            return await _collection.Find(new BsonDocument())
                .SortByDescending(x => x.DonatedAt)
                .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        }

        public async Task Update(Donation donation)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, donation.Id);
            await _collection.ReplaceOneAsync(filter, donation);
        }

        public async Task Delete(Guid id)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Id, id);
            await _collection.DeleteOneAsync(filter);
        }

        public async Task<Donation> GetByTransactionId(string transactionId)
        {
            return await _collection.Find(x => x.TransactionId == transactionId).FirstOrDefaultAsync();
        }

        public async Task<List<Donation>> GetByCampaignId(Guid campaignId, DonationBalanceStatus donationBalanceStatus)
        {
            var filter = Builders<Donation>.Filter.And(
                Builders<Donation>.Filter.Eq(d => d.CampaignId, campaignId),
                Builders<Donation>.Filter.Eq(d => d.BalanceStatus, donationBalanceStatus));

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Donation>> GetByBalanceStatus(DonationBalanceStatus donationBalanceStatus)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.BalanceStatus, donationBalanceStatus);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task UpdateMany(List<Donation> donations)
        {
            var bulkOperations = new List<WriteModel<Donation>>();

            foreach (var donation in donations)
            {
                var filter = Builders<Donation>.Filter.Eq(d => d.Id, donation.Id);
                var update = Builders<Donation>.Update
                    .Set(d => d.BalanceStatus, donation.BalanceStatus);

                var updateOne = new UpdateOneModel<Donation>(filter, update);
                bulkOperations.Add(updateOne);
            }

            await _collection.BulkWriteAsync(bulkOperations);
        }

        public async Task<List<Donation>> GetByCampaignId(Guid campaignId, DonationStatus status)
        {
            var filter = Builders<Donation>.Filter.Eq(d => d.Status, status);

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<Donation>> GetByCampaignId(Guid campaignId)
        {
            return await _collection.Find(x => x.CampaignId == campaignId).ToListAsync();
        }

        public async Task<decimal> GetConversionRate()
        {
            var donationsWithUsers = await (
                from donations in _collection.AsQueryable()
                join users in _collectionUsers.AsQueryable()
                    on donations.DonorId equals users.Id
                where !users.Mock && donations.Type == DonationType.Money // ou 0, se for enum
                select new { donations.Status }
            ).ToListAsync();

            if (donationsWithUsers.Count == 0)
                return 0m;

            var totalIntentions = donationsWithUsers.Count(x => x.Status == DonationStatus.Created); // Status == 0
            var totalPaid = donationsWithUsers.Count(x => x.Status == DonationStatus.Paid); // Status == 1

            if (totalIntentions == 0)
                return 0m;

            return (decimal)totalPaid / totalIntentions * 100;
        }
    }
}

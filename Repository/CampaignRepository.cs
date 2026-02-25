using Domain;
using Domain.Interfaces.Repository;
using Domain.Objects;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

namespace Repository
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Campaign> _collection;
        private readonly IMongoCollection<Donation> _donationCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMongoCollection<User> _userCollection;

        public CampaignRepository(IMongoClient mongoClient, string databaseName)
        {
            _mongoClient = mongoClient;
            var database = _mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<Campaign>(Campaign.TABLE_NAME);
            _donationCollection = database.GetCollection<Donation>(Donation.TABLE_NAME);
            _categoryCollection = database.GetCollection<Category>(Category.TABLE_NAME);
            _userCollection = database.GetCollection<User>(User.TABLE_NAME);
        }

        public async Task Add(Campaign campaign) => await _collection.InsertOneAsync(campaign);

        public async Task<Campaign> GetById(Guid id) => await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task Update(Campaign campaign) => await _collection.ReplaceOneAsync(c => c.Id == campaign.Id, campaign);

        public async Task Delete(Guid id) => await _collection.DeleteOneAsync(c => c.Id == id);

        public async Task<PagedResult<Campaign>> GetCampaigns(
            DateTime? startDate, DateTime? endDate, Guid? categoryId, string? title, List<Guid> ids,
            CampaignStatus? status,
            bool? listing,
            int page, int pageSize)
        {
            try
            {
                var filterBuilder = Builders<Campaign>.Filter;
                var filters = new List<FilterDefinition<Campaign>>();

                if (startDate.HasValue)
                    filters.Add(filterBuilder.Gte(c => c.CreatedAt, startDate.Value));

                if (endDate.HasValue)
                    filters.Add(filterBuilder.Lte(c => c.CreatedAt, endDate.Value));

                if (categoryId != null)
                    filters.Add(filterBuilder.Eq(c => c.CategoryId, categoryId));

                if (!string.IsNullOrEmpty(title))
                    filters.Add(filterBuilder.Regex(c => c.Title, new BsonRegularExpression(title, "i")));

                if (ids != null && ids.Count != 0)
                    filters.Add(filterBuilder.In(c => c.Id, ids));

                if(status != null)
                    filters.Add(filterBuilder.Eq(c => c.Status, status));

                if (listing.HasValue)
                {
                    // Por padrão, exclui campanhas com Listing = false
                    filters.Add(filterBuilder.Or(
                        filterBuilder.Exists(c => c.Listing, false), // não tem o campo
                        filterBuilder.Ne(c => c.Listing, false)      // ou é diferente de false
                    ));

                }

                var combinedFilter = filters.Count > 0
                    ? filterBuilder.And(filters)
                    : FilterDefinition<Campaign>.Empty;


                var totalItems = await _collection.CountDocumentsAsync(combinedFilter);

                var campaignsWithDetails = await _collection.Aggregate()
                .Match(combinedFilter)
                .SortByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Lookup<Campaign, Donation, Campaign>(
                    _donationCollection,
                    c => c.Id,
                    d => d.CampaignId,
                    c => c.Donations)
                .Lookup<Campaign, Category, Campaign>(
                    _categoryCollection,
                    c => c.CategoryId,
                    cat => cat.Id,
                    c => c.Category)
                .Lookup<Campaign, User, Campaign>(
                    _userCollection,
                    c => c.CreatorId,
                    user => user.Id,
                    c => c.Creator)
                .Unwind<Campaign, Campaign>(c => c.Category, new AggregateUnwindOptions<Campaign> { PreserveNullAndEmptyArrays = true })
                .Unwind<Campaign, Campaign>(c => c.Creator, new AggregateUnwindOptions<Campaign> { PreserveNullAndEmptyArrays = true })
                .ToListAsync();

                var hasMoreItems = (page * pageSize) < totalItems;

                return new PagedResult<Campaign>
                {
                    Items = campaignsWithDetails,
                    TotalItems = totalItems,
                    HasMoreItems = hasMoreItems
                };
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }


        public async Task<PagedResult<Campaign>> GetCampaigns(DateTime? startDate, DateTime? endDate, 
            string? name, CampaignStatus? status, bool? listing, int page, int pageSize)
        {
            var filterBuilder = Builders<Campaign>.Filter;
            var filters = new List<FilterDefinition<Campaign>>();

            if (startDate.HasValue)
                filters.Add(filterBuilder.Gte(c => c.CreatedAt, startDate.Value));

            if (endDate.HasValue)
                filters.Add(filterBuilder.Lte(c => c.CreatedAt, endDate.Value));

            if(status != null)
                filters.Add(filterBuilder.Eq(c => c.Status, status));

            if (!string.IsNullOrEmpty(name))
                filters.Add(filterBuilder.Regex(c => c.Title, new BsonRegularExpression(name, "i")));

            if (filters.Count == 0)
                filters.Add(filterBuilder.Empty);

            if (listing.HasValue)
            {
                // Por padrão, exclui campanhas com Listing = false
                filters.Add(filterBuilder.Or(
                    filterBuilder.Exists(c => c.Listing, false), // não tem o campo
                    filterBuilder.Ne(c => c.Listing, false)      // ou é diferente de false
                ));

            }

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

            return new PagedResult<Campaign>
            {
                Items = items,
                TotalItems = totalItems,
                HasMoreItems = hasMoreItems
            };
        }

        public async Task<List<Campaign>> GetByCreatorId(Guid creatorId, int page, int pageSize)
        {
            return await _collection
            .Find(c => c.CreatorId == creatorId)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        }

        public async Task<bool> ExistSlug(string slug)
        {
            var campaign = await _collection.Find(x => x.Slug == slug).FirstOrDefaultAsync();
            return campaign != null;
        }

        public async Task<Campaign> GetBySlug(string slug) => await _collection.Find(c => c.Slug == slug).FirstOrDefaultAsync();

        public async Task<List<Campaign>> GetByCreatorId(Guid creatorId)
        {
            return await _collection
            .Find(c => c.CreatorId == creatorId)
            .ToListAsync();
        }

        public async Task<List<Campaign>> GetCampaignsByStatusAndMinAge(CampaignStatus status, DateTime date, int daysBefore)
        {
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(x => x.Status, status),
                Builders<Campaign>.Filter.Lte(x => x.CreatedAt, date.AddDays(-daysBefore))
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task RenameNewPlanIdToCurrentPlanIdAsync()
        {
            var filter = Builders<Campaign>.Filter.Exists("NewPlanId"); // Filtra apenas documentos que possuem NewPlanId
            var update = Builders<Campaign>.Update.Rename("NewPlanId", "CurrentPlanId");

            var result = await _collection.UpdateManyAsync(filter, update);

            Console.WriteLine($"Documentos atualizados: {result.ModifiedCount}");

            var remove = Builders<Campaign>.Update.Unset("NewPlanId");
            await _collection.UpdateManyAsync(Builders<Campaign>.Filter.Exists("NewPlanId"), remove);

        }

        public async Task RenameNewPercentToBeChargedToCurrentPercentToBeCharged()
        {
            var filter = Builders<Campaign>.Filter.Exists("NewPercentToBeCharged"); // Filtra apenas documentos que possuem NewPlanId
            var update = Builders<Campaign>.Update.Rename("NewPercentToBeCharged", "CurrentPercentToBeCharged");

            var result = await _collection.UpdateManyAsync(filter, update);

            var remove = Builders<Campaign>.Update.Unset("NewPercentToBeCharged");
            await _collection.UpdateManyAsync(Builders<Campaign>.Filter.Exists("NewPercentToBeCharged"), remove);

            Console.WriteLine($"Documentos atualizados: {result.ModifiedCount}");
        }

        public async Task<long> TotalCampaigns()
        {
            long totalItems = await _collection.CountDocumentsAsync(Builders<Campaign>.Filter.Empty);
            return totalItems;
        }

        public async Task<long> TotalActiveCampaigns()
        {
            var filter = Builders<Campaign>.Filter.And(
                Builders<Campaign>.Filter.Eq(x => x.Status, CampaignStatus.Active));

            long totalItems = await _collection.CountDocumentsAsync(filter);
            return totalItems;
        }
    }
}
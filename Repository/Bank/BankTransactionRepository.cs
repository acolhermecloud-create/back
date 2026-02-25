using Domain;
using Domain.Bank;
using Domain.Interfaces.Repository.Bank;
using Domain.Objects;
using Domain.Objects.Aggregations;
using iugu.net.Lib;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository.Bank
{
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private readonly IMongoCollection<BankTransaction> _collection;
        private readonly IMongoCollection<BankAccount> _bankAccountCollection;
        private readonly IMongoCollection<User> _userCollection;

        public BankTransactionRepository(IMongoClient mongoClient, string databaseName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<BankTransaction>(BankTransaction.TABLE_NAME);
            _bankAccountCollection = database.GetCollection<BankAccount>(BankAccount.TABLE_NAME);
            _userCollection = database.GetCollection<User>(User.TABLE_NAME);
        }

        public async Task<BankTransaction?> GetById(Guid id)
        {
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<PagedResult<BankTransaction>> GetTransactionByType(Guid accountId, BankTransactionType transactionType, int page, int pageSize)
        {
            var flter = Builders<BankTransaction>.Filter.And(
                Builders<BankTransaction>.Filter.Eq(x => x.AccountId, accountId),
                Builders<BankTransaction>.Filter.Eq(x => x.BankTransactionType, transactionType));
            var totalItems = await _collection.CountDocumentsAsync(flter);

            var items = await _collection.Find(flter)
                .SortByDescending(doc => doc.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var hasMoreItems = (page * pageSize) < totalItems;

            return new PagedResult<BankTransaction>
            {
                Items = items,
                TotalItems = totalItems,
                HasMoreItems = hasMoreItems
            };
        }

        public async Task Create(BankTransaction transaction)
        {
            await _collection.InsertOneAsync(transaction);
        }

        public async Task Update(BankTransaction transaction)
        {
            var update = Builders<BankTransaction>.Update
                .Set(t => t.Status, transaction.Status)
                .Set(t => t.ReasonToFailed, transaction.ReasonToFailed)
                .Set(t => t.ProcessedAt, DateTime.Now);

            await _collection.UpdateOneAsync(t => t.Id == transaction.Id, update);
        }

        public async Task<List<BankTransaction>> GetBankTransactionsWaitingRelease()
        {
            var filter = Builders<BankTransaction>.Filter.And(
                Builders<BankTransaction>.Filter.Eq(x => x.BankTransactionType, BankTransactionType.CashIn),
                Builders<BankTransaction>.Filter.Eq(x => x.Status, BankTransactionStatus.WaitingForRelease));

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<BankTransaction>> GetBankTransactions(Guid accountId, BankTransactionStatus status, BankTransactionType bankTransactionType)
        {
            var filter = Builders<BankTransaction>.Filter.And(
                Builders<BankTransaction>.Filter.Eq(x => x.AccountId, accountId),
                Builders<BankTransaction>.Filter.Eq(x => x.Status, status),
                Builders<BankTransaction>.Filter.Eq(x => x.BankTransactionType, BankTransactionType.CashIn));

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<BankTransaction>> GetDailyBankTransactions(
            Guid accountId,
            BankTransactionStatus status,
            BankTransactionType bankTransactionType,
            DateTime date)
        {
            var startDate = date.Date; // Início do dia (00:00:00)
                                       // Define o final do dia como 23:59:59
            var endDate = date.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var filter = Builders<BankTransaction>.Filter.And(
                Builders<BankTransaction>.Filter.Eq(x => x.AccountId, accountId),
                Builders<BankTransaction>.Filter.Eq(x => x.Status, status),
                Builders<BankTransaction>.Filter.Eq(x => x.BankTransactionType, bankTransactionType),
                Builders<BankTransaction>.Filter.Gte(x => x.CreatedAt, startDate),
                Builders<BankTransaction>.Filter.Lte(x => x.CreatedAt, endDate)
            );

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<BankTransaction>> GetTransactionByType(Guid accountId, BankTransactionType transactionType,
            List<BankTransactionStatus> transactionStatuses)
        {
            var flter = Builders<BankTransaction>.Filter.And(
                Builders<BankTransaction>.Filter.In(x => x.Status, transactionStatuses),
                Builders<BankTransaction>.Filter.Eq(x => x.AccountId, accountId),
                Builders<BankTransaction>.Filter.Eq(x => x.BankTransactionType, transactionType));

            return await _collection.Find(flter).ToListAsync();
        }

        public async Task<List<BankTransaction>> GetTransactions(DateTime startDate, DateTime endDate, BankTransactionType type)
        {
            var filterBuilder = Builders<BankTransaction>.Filter;

            var filters = new List<FilterDefinition<BankTransaction>>
            {
                filterBuilder.Gte(t => t.CreatedAt, startDate), // Data >= startDate
                filterBuilder.Lte(t => t.CreatedAt, endDate),   // Data <= endDate
                filterBuilder.Eq(t => t.BankTransactionType, type)        // Tipo específico
            };

            var combinedFilter = filterBuilder.And(filters);

            return await _collection.Find(combinedFilter)
                .SortByDescending(t => t.CreatedAt) // Ordena da mais recente para a mais antiga
                .ToListAsync();
        }

        public async Task<List<BankTransaction>> GetAllTransactionsExceptTheSystemOne(
            Guid systemAccountId, DateTime startDate,
            DateTime endDate, BankTransactionType type)
        {
            var filterBuilder = Builders<BankTransaction>.Filter;

            var filters = new List<FilterDefinition<BankTransaction>>
            {
                filterBuilder.Gte(t => t.CreatedAt, startDate), // Data >= startDate
                filterBuilder.Lte(t => t.CreatedAt, endDate),   // Data <= endDate
                filterBuilder.Eq(t => t.BankTransactionType, type),       // Tipo específico
                filterBuilder.Ne(t => t.AccountId, systemAccountId) // Exclui transações do sistema
            };

            var combinedFilter = filterBuilder.And(filters);

            return await _collection.Find(combinedFilter)
                .SortByDescending(t => t.CreatedAt) // Ordena da mais recente para a mais antiga
                .ToListAsync();
        }

        public async Task<List<BankTransaction>> GetTransactions(DateTime startDate, DateTime endDate, BankTransactionType type,
            BankTransactionSource bankTransactionSource)
        {
            var filterBuilder = Builders<BankTransaction>.Filter;

            var filters = new List<FilterDefinition<BankTransaction>>
            {
                filterBuilder.Gte(t => t.CreatedAt, startDate), // Data >= startDate
                filterBuilder.Lte(t => t.CreatedAt, endDate),   // Data <= endDate
                filterBuilder.Eq(t => t.BankTransactionType, type),       // Tipo específico
                filterBuilder.Eq(t => t.BankTransactionSource, bankTransactionSource) // Exclui transações do sistema
            };

            var combinedFilter = filterBuilder.And(filters);

            return await _collection.Find(combinedFilter)
                .SortByDescending(t => t.CreatedAt) // Ordena da mais recente para a mais antiga
                .ToListAsync();
        }

        public async Task<PagedResult<BankTransactionWithAccountDto>> GetTransactionByType(
            List<BankTransactionStatus> transactionStatuses,
            List<BankTransactionType> transactionTypes,
            int page,
            int pageSize,
            Guid? accountIdToExclude = null)
        {
            try
            {
                var filterBuilder = Builders<BankTransaction>.Filter;
                var filters = new List<FilterDefinition<BankTransaction>>
                {
                    filterBuilder.In(x => x.Status, transactionStatuses),
                    filterBuilder.In(x => x.BankTransactionType, transactionTypes)
                };

                if (accountIdToExclude != null)
                {
                    filters.Add(filterBuilder.Ne(x => x.AccountId, accountIdToExclude));
                }

                var finalFilter = filterBuilder.And(filters);
                var totalItems = await _collection.CountDocumentsAsync(finalFilter);

                var documents = await _collection.Aggregate()
               .Match(finalFilter)
               .SortByDescending(x => x.CreatedAt)
               .Skip((page - 1) * pageSize)
               .Limit(pageSize)
               .Lookup(
                   foreignCollection: _bankAccountCollection,
                   localField: x => x.AccountId,
                   foreignField: x => x.Id,
                   @as: (BankTransactionWithAccountJoin temp) => temp.BankAccount)
               .Unwind<BankTransactionWithAccountJoin, BankTransactionWithAccountJoin>(x => x.BankAccount)
               .Lookup(
                   foreignCollection: _userCollection,
                   localField: x => x.BankAccount.UserId,
                   foreignField: x => x.Id,
                   @as: (BankTransactionWithAccountJoin temp) => temp.User)
               .Unwind<BankTransactionWithAccountJoin, BankTransactionWithAccountJoin>(x => x.User)
               .Project(x => new BankTransactionWithAccountDto
               {
                   Id = x.Id,
                   AccountId = x.AccountId,
                   DonationId = x.DonationId,
                   Gross = x.Gross,
                   Tax = x.Tax,
                   Liquid = x.Liquid,
                   BankTransactionType = x.BankTransactionType,
                   Status = x.Status,
                   BankTransactionSource = x.BankTransactionSource,
                   Description = x.Description,
                   ReasonToFailed = x.ReasonToFailed,
                   CreatedAt = x.CreatedAt,
                   ProcessedAt = x.ProcessedAt,
                   BankAccount = x.BankAccount,
                   User = new User
                   {
                       Id = x.User.Id,
                       Name = x.User.Name,
                       Email = x.User.Email,
                       DocumentId = x.User.DocumentId,
                   }
               })
               .ToListAsync();

                var hasMoreItems = (page * pageSize) < totalItems;

                return new PagedResult<BankTransactionWithAccountDto>
                {
                    Items = documents,
                    TotalItems = totalItems,
                    HasMoreItems = hasMoreItems
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

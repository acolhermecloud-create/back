using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Bank
{
    public class BankTransaction(
        Guid accountId, 
        Guid? donationId, 
        decimal gross, 
        decimal tax, 
        decimal liquid, 
        BankTransactionType type,
        BankTransactionStatus bankTransactionStatus,
        BankTransactionSource bankTransactionSource,
        string? description,
        string? reasonToFailed,
        DateTime? processedAt)
    {
        // Construtor vazio que utiliza valores padrão para os parâmetros do construtor principal
        public BankTransaction() : this(Guid.Empty, Guid.Empty, 0, 0, 0, BankTransactionType.CashIn, BankTransactionStatus.Pending,
            BankTransactionSource.Campaign, string.Empty, string.Empty, DateTime.Now)
        {
        }

        public static readonly string TABLE_NAME = "bank_transactions";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid AccountId { get; set; } = accountId; // Conta bancária relacionada

        [BsonRepresentation(BsonType.String)]
        public Guid? DonationId { get; set; } = donationId;

        public decimal Gross { get; set; } = gross;
        public decimal Tax { get; set; } = tax;
        public decimal Liquid { get; set; } = liquid;

        public BankTransactionType BankTransactionType { get; set; } = type;
        public BankTransactionStatus Status { get; set; } = bankTransactionStatus;
        public BankTransactionSource BankTransactionSource { get; set; } = bankTransactionSource;

        public string? Description { get; set; } = description; // Opcional: info extra da transação
        public string? ReasonToFailed { get; set; } = reasonToFailed;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; } = processedAt;
    }
}

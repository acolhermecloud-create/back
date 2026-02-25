using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Bank
{
    public class BankAccount(Guid userId, string pixKey, Int64 balance, BankAccountType bankAccountType)
    {
        public static readonly string TABLE_NAME = "bank_account";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } = userId;

        //public string BankName { get; set; } = string.Empty;
        //public string BankCode { get; set; } = string.Empty;
        //public string Agency { get; set; } = string.Empty;
        //public string AccountNumber { get; set; } = string.Empty;
        public string? PixKey { get; set; } = pixKey;

        public decimal Balance { get; set; } = balance; // Saldo da conta
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public BankAccountType BankAccountType { get; set; } = bankAccountType;
    }
}

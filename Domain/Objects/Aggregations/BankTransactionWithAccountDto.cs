using Domain.Bank;

namespace Domain.Objects.Aggregations
{
    public class BankTransactionWithAccountDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? DonationId { get; set; }

        public decimal Gross { get; set; }
        public decimal Tax { get; set; }
        public decimal Liquid { get; set; }

        public BankTransactionType BankTransactionType { get; set; }
        public BankTransactionStatus Status { get; set; }
        public BankTransactionSource BankTransactionSource { get; set; }

        public string? Description { get; set; }
        public string? ReasonToFailed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public BankAccount BankAccount { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}

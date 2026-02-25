using Domain.Bank;

namespace Domain.Objects.Aggregations
{
    public class BankTransactionWithAccountJoin : BankTransaction
    {
        public BankAccount BankAccount { get; set; }
        public User User { get; set; }
    }
}

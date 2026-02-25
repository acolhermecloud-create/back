using Domain.Acquirers;

namespace Domain.Interfaces.Repository.Acquirers
{
    public interface IReflowPayRepository
    {
        Task Add(ReflowPay reflowPay);

        Task Update(ReflowPay reflowPay);

        Task<ReflowPay> GetFees();
    }
}

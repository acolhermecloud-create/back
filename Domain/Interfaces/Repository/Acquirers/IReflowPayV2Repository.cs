using Domain.Acquirers;

namespace Domain.Interfaces.Repository.Acquirers
{
    public interface IReflowPayV2Repository
    {
        Task Add(ReflowPayV2 reflowPay);

        Task Update(ReflowPayV2 reflowPay);

        Task<ReflowPayV2> GetFees();
    }
}

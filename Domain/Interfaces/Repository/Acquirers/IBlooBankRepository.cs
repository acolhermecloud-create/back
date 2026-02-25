using Domain.Acquirers;

namespace Domain.Interfaces.Repository.Acquirers
{
    public interface IBlooBankRepository
    {
        Task Add(BlooBank bloobank);

        Task UpdateFees(BlooBank bloobank);

        Task<BlooBank> GetFees();
    }
}

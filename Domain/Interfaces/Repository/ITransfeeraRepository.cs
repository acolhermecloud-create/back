using Domain.Acquirers;

namespace Domain.Interfaces.Repository
{
    public interface ITransfeeraRepository
    {
        Task AddOrUpdate(Transfeera transfeera);

        Task<Transfeera> Get();
    }
}

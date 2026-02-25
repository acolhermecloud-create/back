using Domain.Objects;

namespace Domain.Interfaces.Repository
{
    public interface IPlanRepository
    {
        Task Add(Plan plan);

        Task Update(Plan plan);

        Task<Plan> GetById(Guid planId);

        Task Desactive(Guid planId);

        Task<PagedResult<Plan>> GetAll(int page, int pageSize);

        Task<List<Plan>> GetAll();

        Task<Plan> GetDefault();
    }
}

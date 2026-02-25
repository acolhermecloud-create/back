using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface IPlanService
    {
        Task<List<Plan>> ListPlans();

        Task<PagedResult<Plan>> ListPlans(int page, int pageSize);

        Task UpdatePlan(Guid id, string title, string description, 
            string[] benefits, decimal percent, decimal fixedRate, bool needApproval, bool @default, bool status);
    }
}

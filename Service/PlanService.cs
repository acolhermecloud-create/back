using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Domain.Objects;

namespace Service
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;

        public PlanService(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        public async Task<List<Plan>> ListPlans()
        {
            return await _planRepository.GetAll();
        }

        public async Task<PagedResult<Plan>> ListPlans(int page, int pageSize)
        {
            return await _planRepository.GetAll(page, pageSize);
        }

        public async Task UpdatePlan(Guid id, string title, string description, 
            string[] benefits, decimal percent, decimal fixedRate,
            bool needApproval, bool @default, bool status)
        {
            var plan = await _planRepository.GetById(id);
            if (plan == null) throw new Exception("Nenhum plano encontrado");

            plan.Title = title;
            plan.Description = description;
            plan.Benefits = benefits;
            plan.PercentToBeCharged = percent;
            plan.FixedRate = fixedRate;
            plan.NeedApproval = needApproval;
            plan.Default = @default;
            plan.Active = status;

            await _planRepository.Update(plan);
        }
    }
}

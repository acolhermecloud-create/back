using Domain.System;

namespace Domain.Interfaces.Repository
{
    public interface IGatewayConfigurationRepository
    {
        Task<GatewayConfiguration> Get();
        Task<GatewayConfiguration> GetByIdAsync(Guid id);
        Task Add(GatewayConfiguration configuration);
        Task Update(GatewayConfiguration configuration);
        Task DeleteAsync(Guid id);
    }
}

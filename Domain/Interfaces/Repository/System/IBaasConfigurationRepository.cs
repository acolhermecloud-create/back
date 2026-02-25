using Domain.System;

namespace Domain.Interfaces.Repository.System
{
    public interface IBaasConfigurationRepository
    {
        Task Add(BaasConfiguration config); 
        Task<BaasConfiguration> Get();
    }
}

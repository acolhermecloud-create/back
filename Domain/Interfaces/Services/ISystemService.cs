using Domain.Acquirers;
using Domain.System;

namespace Domain.Interfaces.Services
{
    public interface ISystemService
    {
        Task<GatewayConfiguration> GetGatewayConfiguration();

        Task UpdateConfiguration(Gateway pix, Gateway card, bool tryToGenerateCashInInOtherAcquirers);

        Task<ReflowPay> GetReflowPayData();

        Task<ReflowPayV2> GetReflowPayV2Data();

        Task<BlooBank> GetBloobankData();

        Task<Transfeera> GetTransfeeraData();

        Task UpdateReflowPayData(decimal fixedRate, decimal variableRate);

        Task UpdateReflowPayV2Data(decimal fixedRate, decimal variableRate);

        Task UpdateBloobankData(decimal fixedRate, decimal variableRate);

        Task UpdateTransfeeraAcquirerData(decimal acquirerFixedRate, decimal acquirerVariableRate);
    }
}

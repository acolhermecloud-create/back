using Domain;
using Domain.Acquirers;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.Acquirers;
using Domain.Interfaces.Services;
using Domain.System;

namespace Service
{
    public class SystemService : ISystemService
    {
        private readonly IGatewayConfigurationRepository _configurationRepository;

        private readonly ITransfeeraRepository _transfeeraRepository;
        private readonly IBlooBankRepository _blooBankRepository;
        private readonly IReflowPayRepository _reflowPayRepository;
        private readonly IReflowPayV2Repository _reflowPayV2Repository;

        public SystemService(IGatewayConfigurationRepository gatewayConfigurationRepository,
            ITransfeeraRepository transfeeraRepository,
            IBlooBankRepository blooBankRepository,
            IReflowPayRepository reflowPayRepository,
            IReflowPayV2Repository reflowPayV2Repository)
        {
            _configurationRepository = gatewayConfigurationRepository;

            _transfeeraRepository = transfeeraRepository;
            _blooBankRepository = blooBankRepository;
            _reflowPayRepository = reflowPayRepository;
            _reflowPayV2Repository = reflowPayV2Repository;
        }

        public async Task<BlooBank> GetBloobankData()
            => await _blooBankRepository.GetFees();

        public async Task<GatewayConfiguration> GetGatewayConfiguration()
            => await _configurationRepository.Get();

        public async Task<ReflowPay> GetReflowPayData()
            => await _reflowPayRepository.GetFees();

        public async Task<ReflowPayV2> GetReflowPayV2Data()
            => await _reflowPayV2Repository.GetFees();

        public async Task<Transfeera> GetTransfeeraData()
            => await _transfeeraRepository.Get();

        public async Task UpdateConfiguration(Gateway pix, Gateway card, bool tryToGenerateCashInInOtherAcquirers)
        {
            var configuration = await _configurationRepository.Get();
            configuration.Pix = pix;
            configuration.Card = card;
            configuration.TryToGenerateCashInInOtherAcquirers = tryToGenerateCashInInOtherAcquirers;

            await _configurationRepository.Update(configuration);
        }

        public async Task UpdateReflowPayData(decimal fixedRate, decimal variableRate)
        {
            var reflow = await _reflowPayRepository.GetFees();

            if (reflow != null)
            {
                reflow.FixedRate = fixedRate;
                reflow.VariableRate = variableRate;
                await _reflowPayRepository.Update(reflow);
            }
            else
            {
                reflow = new();
                reflow.FixedRate = fixedRate;
                reflow.VariableRate = variableRate;
                await _reflowPayRepository.Add(reflow);
            }
        }

        public async Task UpdateReflowPayV2Data(decimal fixedRate, decimal variableRate)
        {
            var reflowV2 = await _reflowPayV2Repository.GetFees();

            if (reflowV2 != null)
            {
                reflowV2.FixedRate = fixedRate;
                reflowV2.VariableRate = variableRate;
                await _reflowPayV2Repository.Update(reflowV2);
            }
            else
            {
                reflowV2 = new();
                reflowV2.FixedRate = fixedRate;
                reflowV2.VariableRate = variableRate;
                await _reflowPayV2Repository.Add(reflowV2);
            }
        }

        public async Task UpdateTransfeeraAcquirerData(decimal acquirerFixedRate, decimal acquirerVariableRate)
        {
            var transfeera = await _transfeeraRepository.Get();
            
            if(transfeera != null)
            {
                transfeera.AcquirerVariableRate = acquirerVariableRate;
                transfeera.AcquirerFixedRate = acquirerFixedRate;
                await _transfeeraRepository.AddOrUpdate(transfeera);
            }
            else
            {
                transfeera = new();
                transfeera.AcquirerVariableRate = acquirerVariableRate;
                transfeera.AcquirerFixedRate = acquirerFixedRate;
                await _transfeeraRepository.AddOrUpdate(transfeera);
            }
        }

        public async Task UpdateBloobankData(decimal fixedRate, decimal variableRate)
        {
            var bloobank = await _blooBankRepository.GetFees();

            if(bloobank != null)
            {
                bloobank.FixedRate = fixedRate;
                bloobank.VariableRate = variableRate;
                await _blooBankRepository.UpdateFees(bloobank);
            }
            else
            {
                bloobank = new();
                bloobank.FixedRate = fixedRate;
                bloobank.VariableRate = variableRate;
                await _blooBankRepository.Add(bloobank);
            }
        }
    }
}

using Domain.Objects.Venit;

namespace Domain.Interfaces.Services
{
    public interface IVenitService
    {
        Task<ResponseConsultPixKey?> ConsultPixKeyAsync(string pixKey);
        Task<long> GetBalance();
        Task<ResponseTransferMoney> SendPixAsync(ResponseConsultPixKey responseConsultPixKey, decimal value);
    }
}

namespace Domain.Interfaces.Services
{
    public interface IUtmfyService
    {
        Task<string> SendEvent(Utm utm);
    }
}

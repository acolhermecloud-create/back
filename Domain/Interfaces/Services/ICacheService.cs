namespace Domain.Interfaces.Services
{
    public interface ICacheService
    {
        Task Delete(string key);

        Task<string?> Get(string key);

        Task Set(string key, string value, double hours);

        Task SetMinutes(string key, string value, int minutes);
    }
}

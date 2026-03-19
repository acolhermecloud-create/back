namespace Domain.Interfaces.Services
{
    public interface IFacebookPixelService
    {
        Task<bool> SendEventToFacebookAsync(string pixelId, string accessToken, string eventName, string eventId, Utm eventParams);
    }
}

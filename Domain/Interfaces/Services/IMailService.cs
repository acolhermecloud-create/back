namespace Domain.Interfaces.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}

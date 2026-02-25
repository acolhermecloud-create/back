namespace Domain.Interfaces.Services
{
    public interface IEmailService
    {
        Task Send(string[] to,
            string subject,
            string body,
            string businessAddress,
            string businessName,
            Dictionary<string, string>? base64Attachments);
    }
}

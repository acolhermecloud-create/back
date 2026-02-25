namespace API.Payloads
{
    public class NotificationPayload
    {
        public List<string>? UsersId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Data { get; set; }
    }
}
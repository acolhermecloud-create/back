namespace API.Payloads
{
    public class PasswordPayload
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

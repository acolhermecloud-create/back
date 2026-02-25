namespace API.Payloads
{
    public class ForgotPasswordPayload
    {
        public string UserMail { get; set; }
        public string Challenge { get; set; }
        public string NewPassword { get; set; }
    }
}

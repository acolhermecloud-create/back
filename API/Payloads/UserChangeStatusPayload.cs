using Domain;

namespace API.Payloads
{
    public class UserChangeStatusPayload
    {
        public string UserId { get; set; }
        public UserStatus UserStatus { get; set; }
    }
}

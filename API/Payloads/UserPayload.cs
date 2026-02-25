using Domain;

namespace API.Payloads
{
    public class UserPayload
    {
        public string? Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DocumentId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public AuthProvider Provider { get; set; } = AuthProvider.None;
        public UserType Type { get; set; } = UserType.Common;
    }
}

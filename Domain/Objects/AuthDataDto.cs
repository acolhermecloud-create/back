namespace Domain.Objects
{
    public class AuthDataDto
    {
        public bool TwoFactorActive { get; set; }
        public string Token { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string DocumentId { get; set; }
        public string Avatar { get; set; }
        public Address Address { get; set; }
    }
}

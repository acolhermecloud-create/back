using Domain;

namespace API.Payloads
{
    public class GetUsersFilterPayload
    {
        public string? Name { get; set; }
        public UserType Type { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

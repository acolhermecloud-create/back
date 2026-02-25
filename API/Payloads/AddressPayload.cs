namespace API.Payloads
{
    public class AddressPayload
    {
        public string UserId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; }
    }
}
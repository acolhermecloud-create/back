namespace API.Payloads
{
    public class OngPayload
    {
        public string? Id { get; set; }
        public string? AddressId { get; set; }
        public string CategoryId { get; set; }
        public string? OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string About { get; set; }
        public string Site { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public string Instagram { get; set; }
        public string Youtube { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public IFormFile? Banner { get; set; }
    }
}

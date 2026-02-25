using Newtonsoft.Json;

namespace Domain.Objects.ReflowPay
{
    public class TransactionPayload
    {
        [JsonProperty("postbackUrl")]
        public string PostbackUrl { get; set; }

        [JsonProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("shipping")]
        public Shipping? Shipping { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("isInfoProduct")]
        public bool IsInfoProduct { get; set; }
    }

    public class Address
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("streetNumber")]
        public string StreetNumber { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("complement")]
        public string Complement { get; set; }
    }

    public class Customer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("document")]
        public Document Document { get; set; }
    }

    public class Document
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Item
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("unitPrice")]
        public int UnitPrice { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("tangible")]
        public bool Tangible { get; set; }
    }

    public class Shipping
    {
        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }
    }

    public class Card
    {
        public string Number { get; set; }
        public string Expiry { get; set; }
        public string CVV { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Address(string street, string city, string state, string zipCode, string country)
    {
        public static readonly string TABLE_NAME = "addresses"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um novo ID

        public string Street { get; set; } = street;
        public string City { get; set; } = city;
        public string State { get; set; } = state;
        public string ZipCode { get; set; } = zipCode;
        public string Country { get; set; } = country;
    }
}
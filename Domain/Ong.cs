using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Ong(
        string name, 
        string description, 
        string about, 
        string site,
        string mail,
        string phone,
        string instagram,
        string youtube,
        string bannerKey,
        Guid categoryId, 
        Guid addressId,
        Guid ownerId)
    {
        public static readonly string TABLE_NAME = "ongs"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid CategoryId { get; set; } = categoryId;

        [BsonRepresentation(BsonType.String)]
        public Guid AddressId { get; set; } = addressId;

        [BsonRepresentation(BsonType.String)]
        public Guid OwnerId { get; set; } = ownerId;

        public string Name { get; set; } = name;
        public string Description { get; set; } = description;
        public string About { get; set; } = about;
        public string Site { get; set; } = site;
        public string Mail { get; set; } = mail;
        public string Phone { get; set; } = phone;
        public string Instagram { get; set; } = instagram;
        public string Youtube { get; set; } = youtube;
        public string BannerKey { get; set; } = bannerKey;

        [BsonIgnore]
        public Address? Address { get; set; }

        [BsonIgnore]
        public User? User { get; set; }
    }
}

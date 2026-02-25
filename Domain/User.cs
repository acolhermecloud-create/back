using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class User(string name, string email, string password, string documentId, 
        AuthProvider provider, 
        UserType type,
        Guid? addressId = null, string? phone = null)
    {

        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "users";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        [BsonRepresentation(BsonType.String)]
        public Guid? AddressId { get; set; } = addressId;

        // Propriedades
        public string Name { get; set; } = name;
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public string DocumentId { get; set; } = documentId; // CPF
        public string AvatarKey { get; set; } = string.Empty;
        public string? Phone { get; set; } = phone ?? string.Empty;
        public AuthProvider Provider { get; set; } = provider;
        public UserType Type { get; set; } = type;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Usa o tempo local
        public string? TwoFactorSecret { get; set; }

        public bool Mock { get; set; } = false;

        [BsonIgnore]
        public string AvatarUrl { get; set; } = string.Empty;

        [BsonIgnore]
        public Address Address { get; set; }

        [BsonIgnore]
        public Ong? Ong { get; set; }

        [BsonIgnore]
        public UserPoints? UserPoints { get; set; }

        // Construtor vazio que utiliza valores padrão para os parâmetros do construtor principal
        public User() : this(string.Empty, string.Empty, string.Empty, string.Empty,
                              default, default, null)
        {
        }
    }
}
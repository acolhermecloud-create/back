using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserPaymentAccount(Guid userId, string accountId, string? liveApiToken, string? testApiToken, string? userToken, Gateway gateway)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "users";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid UserId { get; set; } = userId;

        public string AccountId { get; set; } = accountId;
        public string? LiveApiToken { get; set; } = liveApiToken;
        public string? TestApiToken { get; set; } = testApiToken;
        public string? UserToken { get; set; } = userToken;
        public Gateway Gateway { get; set; } = gateway;
        public bool Active { get; set; } = true;
    }
}

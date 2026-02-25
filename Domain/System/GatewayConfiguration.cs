using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.System
{
    public class GatewayConfiguration
    {
        public static readonly string TABLE_NAME = "gateway_configuration"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public Gateway Pix { get; set; } = Gateway.ReflowPayV2;
        public Gateway Card { get; set; } = Gateway.ReflowPayV2;
        public bool TryToGenerateCashInInOtherAcquirers { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

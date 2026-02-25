using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Acquirers
{
    public class ReflowPayV2
    {
        public static readonly string TABLE_NAME = "acquirer_reflow_v2"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public decimal FixedRate { get; set; }
        public decimal VariableRate { get; set; }
    }
}

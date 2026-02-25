using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Acquirers
{
    public class Transfeera
    {
        public static readonly string TABLE_NAME = "transfeera"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public decimal AcquirerFixedRate { get; set; }
        public decimal AcquirerVariableRate { get; set; }
        public decimal BaasFixedRate { get; set; }
        public decimal BaasVariableRate { get; set; }
        public string WebhookCashInSecret { get; set; }
        public string WebhookTransferSecret { get; set; }
    }
}

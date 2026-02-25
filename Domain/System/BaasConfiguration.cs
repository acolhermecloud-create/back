using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.System
{
    public class BaasConfiguration
    {
        public static readonly string TABLE_NAME = "baas_configuration"; // Nome da coleção no MongoDB

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public bool AnalyseWithdraw { get; set; } = true;
        public long DailyWithdrawalLimitValue { get; set; } = 1000000;
        public long DailyWithdrawalMinimumValue { get; set; } = 1000;
    }
}

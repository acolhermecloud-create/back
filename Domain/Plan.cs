using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class Plan(string title, string description, string[] benefits,
        decimal percentToBeCharged, decimal fixedRate, bool needApproval, bool @default, int position)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "plans";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public string Title { get; set; } = title;
        public string Description { get; set; } = description;
        public string[] Benefits { get; set; } = benefits;
        public decimal PercentToBeCharged { get; set; } = percentToBeCharged;
        public decimal FixedRate { get; set; } = fixedRate;
        public bool NeedApproval { get; set; } = needApproval;
        public bool Default { get; set; } = @default;
        public bool Active { get; set; } = true;
        public PlanType Type { get; set; }
        public int Position { get; set; } = position;
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class DigitalSticker(string description, int price, int qtd, int cashback, bool highlight = false)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "digitalSticker";

        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        public string Description { get; set; } = description;
        public int Price { get; set; } = price;
        public int Qtd { get; set; } = qtd;
        public int CashBack { get; set; } = cashback;
        public bool Highlight { get; set; } = highlight;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.System
{
    public class GroupAccess(string name, string description)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "group_accesses";

        // O _id será tratado pelo MongoDB como identificador
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; set; } = Guid.NewGuid(); // Gera um Id único

        // Nome do grupo de acesso
        public string Name { get; set; } = name;

        // Descrição do grupo de acesso
        public string Description { get; set; } = description;

        // Lista de IDs dos usuários que fazem parte do grupo
        public List<Guid> UserIds { get; set; } = new List<Guid>();

        // Data de criação do grupo
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Define a data de criação
    }
}
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain
{
    public class UserGroupAccess(Guid userId, Guid groupAccessId)
    {
        // Propriedade estática para o nome da coleção no MongoDB
        public static string TABLE_NAME => "user_group_accesses";

        // O _id será tratado pelo MongoDB como identificador
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Garante que o Guid seja armazenado como String no MongoDB
        public Guid Id { get; private set; } = Guid.NewGuid(); // Gera um Id único

        // ID do usuário
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; private set; } = userId;

        // ID do grupo de acesso
        [BsonRepresentation(BsonType.String)]
        public Guid GroupAccessId { get; private set; } = groupAccessId;

        // Data de criação da associação
        public DateTime CreatedAt { get; private set; } = DateTime.Now; // Define a data de criação
    }
}

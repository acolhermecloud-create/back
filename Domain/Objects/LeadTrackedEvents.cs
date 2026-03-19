using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Objects
{
    public class LeadTrackedEvents
    {
        public static string TABLE_NAME => "lead_tracked_event";

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonRepresentation(BsonType.String)]
        public Guid BotId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        public required string HashUserId { get; set; }

        // =========================
        // Identificação do Lead
        // =========================
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // =========================
        // Facebook / Meta Tracking
        // =========================
        public string? Fbp { get; set; }       // cookie _fbp
        public string? Fbc { get; set; }       // fb.1.timestamp.fbclid
        public string? Fbclid { get; set; }    // parâmetro bruto da URL

        // =========================
        // UTMs
        // =========================
        public string? UtmSource { get; set; }
        public string? UtmMedium { get; set; }
        public string? UtmCampaign { get; set; }
        public string? UtmTerm { get; set; }
        public string? UtmContent { get; set; }

        // =========================
        // Origem e Página
        // =========================
        public string? EventSourceUrl { get; set; }

        // =========================
        // Controle / Auditoria
        // =========================
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public string? Email { get; set; }
        [BsonIgnore]
        public string? Phone { get; set; }
        [BsonIgnore]
        public string? Currency { get; set; }
        [BsonIgnore]
        public string? Value { get; set; }
        [BsonIgnore]
        public string? OrderId { get; set; }
    }
}

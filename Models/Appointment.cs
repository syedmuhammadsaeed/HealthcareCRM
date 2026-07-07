using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthcareCRM.Models
{
    /// <summary>
    /// Represents an appointment record — included for the ERD.
    /// Appointment CRUD is NOT implemented in Week 1.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Appointment
    {
        /// <summary>Gets or sets the unique MongoDB ObjectId.</summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the ID of the user who created the appointment.</summary>
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Gets or sets the ID of the associated patient.</summary>
        [BsonElement("patientId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PatientId { get; set; } = string.Empty;

        /// <summary>Gets or sets the scheduled UTC date and time.</summary>
        [BsonElement("scheduledAt")]
        public DateTime ScheduledAt { get; set; }

        /// <summary>Gets or sets optional clinical notes for the appointment.</summary>
        [BsonElement("notes")]
        public string? Notes { get; set; }
    }
}

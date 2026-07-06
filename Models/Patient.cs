using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthcareCRM.Models
{
    /// <summary>
    /// Represents a patient record stored in the Patients MongoDB collection.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Patient
    {
        /// <summary>Gets or sets the unique MongoDB ObjectId.</summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's full name.</summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's date of birth.</summary>
        [BsonElement("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>Gets or sets the patient's gender.</summary>
        [BsonElement("gender")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's phone number.</summary>
        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's address.</summary>
        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>Gets or sets the patient's status.</summary>
        [BsonElement("status")]
        public string Status { get; set; } = "active";

        /// <summary>Gets or sets the UTC date this record was created.</summary>
        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

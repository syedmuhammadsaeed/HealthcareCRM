using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthcareCRM.Models
{
    /// <summary>
    /// Represents a system user stored in the Users MongoDB collection.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class User
    {
        /// <summary>Gets or sets the unique MongoDB ObjectId.</summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's full name.</summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's email address.</summary>
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the PBKDF2-hashed password.</summary>
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Gets or sets the UTC date this record was created.</summary>
        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>Gets or sets the user's role (e.g., Admin, SuperAdmin, Doctor).</summary>
        [BsonElement("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>Gets or sets the user's registration status (e.g., Pending, Approved, Rejected).</summary>
        [BsonElement("status")]
        public string Status { get; set; } = string.Empty;

        // ── Doctor Specific Fields ──

        /// <summary>Gets or sets the doctor's specialization.</summary>
        [BsonElement("specialization")]
        [BsonIgnoreIfNull]
        public string? Specialization { get; set; }

        /// <summary>Gets or sets the doctor's qualification.</summary>
        [BsonElement("qualification")]
        [BsonIgnoreIfNull]
        public string? Qualification { get; set; }

        /// <summary>Gets or sets the doctor's phone number.</summary>
        [BsonElement("phone")]
        [BsonIgnoreIfNull]
        public string? Phone { get; set; }

        /// <summary>Gets or sets the doctor's address.</summary>
        [BsonElement("address")]
        [BsonIgnoreIfNull]
        public string? Address { get; set; }

        /// <summary>Gets or sets the doctor's consultation fee.</summary>
        [BsonElement("fee")]
        [BsonIgnoreIfNull]
        public decimal? Fee { get; set; }

        /// <summary>Gets or sets the doctor's fee currency.</summary>
        [BsonElement("currency")]
        [BsonIgnoreIfNull]
        public string? Currency { get; set; }

        /// <summary>Gets or sets the user's gender.</summary>
        [BsonElement("gender")]
        [BsonIgnoreIfNull]
        public string? Gender { get; set; }

        /// <summary>Gets or sets the user's age.</summary>
        [BsonElement("age")]
        [BsonIgnoreIfNull]
        public int? Age { get; set; }

        /// <summary>Gets or sets the user's profile picture URL.</summary>
        [BsonElement("profilePictureUrl")]
        [BsonIgnoreIfNull]
        public string? ProfilePictureUrl { get; set; }
    }
}

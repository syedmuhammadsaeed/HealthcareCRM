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
    }
}

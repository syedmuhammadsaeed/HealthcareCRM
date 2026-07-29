using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HealthcareCRM.Models
{
    [BsonIgnoreExtraElements]
    public class LabReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("reportName")]
        public string ReportName { get; set; } = string.Empty;

        [BsonElement("dateOrdered")]
        public DateTime DateOrdered { get; set; }

        [BsonElement("orderDoctor")]
        public string OrderDoctor { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Pending";

        [BsonElement("fileUrl")]
        [BsonIgnoreIfNull]
        public string? FileUrl { get; set; }
    }
}

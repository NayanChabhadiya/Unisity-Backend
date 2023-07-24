using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Enrollment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Semester { get; set; }
        public string Grade { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string CourceId { get; set; }
        public Course? Course { get; set;}
        [BsonRepresentation(BsonType.ObjectId)]
        public string StudentId { get; set; }
        public Student? Student { get; set; }
        public bool? IsActive { get; set; }
    }
}

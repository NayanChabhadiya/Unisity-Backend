using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string FacultyId { get; set; }
        public Faculty? Faculties { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string StudentId { get; set; }
        public Student? Student { get; set; }
        public bool? IsActive { get; set; }
    }
}

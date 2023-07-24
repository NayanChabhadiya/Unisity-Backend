using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Material
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string File { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string SubjectId { get; set; }
        public Subject? Subjects { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string FacultyId { get; set; }
        public Faculty? Faculties { get; set; }
        public bool? IsActive { get; set; }

    }
}

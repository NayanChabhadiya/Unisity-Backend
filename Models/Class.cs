using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Class
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Division { get; set; }
        public int No { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string CourseId { get; set; }
        public Course? Course { get; set; }
        public bool? IsActive { get; set; }

    }
}

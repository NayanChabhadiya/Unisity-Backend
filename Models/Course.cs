using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Credits { get; set; }
        public string CourseType { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public bool? IsActive { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Unisity.Models
{
    public class Subject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string CourceId { get; set; }
        public Course? Course { get; set; }
        public bool? IsActive { get; set; }
    }
}

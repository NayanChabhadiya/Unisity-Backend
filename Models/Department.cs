using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Unisity.Models
{
    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrganizationId { get; set; }
        public Organization? Organizations { get; set; }
        public bool? IsActive { get; set; }
    }
}

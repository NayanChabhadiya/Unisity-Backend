using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Subscription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsActive { get; set; }
    }
}

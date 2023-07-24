using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Transactions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string SubscriptionId { get; set; }
        public Subscription? Subscriptions { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrganizationId { get; set; }
        public Organization? Organizations { get; set; }
    }
}

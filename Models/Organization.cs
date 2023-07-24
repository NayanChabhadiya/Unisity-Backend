﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Numerics;

namespace Unisity.Models
{
    public class Organization
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string OwnerName { get; set; }
        public Int64 Contact { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoleId { get; set; }
        public Role? Roles { get; set; }
        public bool? IsActive { get; set; }
    }
}

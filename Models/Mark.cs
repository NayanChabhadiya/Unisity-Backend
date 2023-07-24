using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Unisity.Models
{
    public class Mark
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public int TotalMarks { get; set; }
        public int ObtainedMarks { get; set; }
        public DateTime CreatedAt { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string ExamId { get; set; }
        public Exam? Exams { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string SubjectId { get; set; }
        public Subject? Subjects { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string StudentId { get; set; }
        public Student? Students { get; set;}
        public bool? IsActive { get; set; }
    }
}

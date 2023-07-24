using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarksController : ControllerBase
    {
        private readonly IMongoCollection<Mark> _markCollection;
        private readonly IMongoCollection<Exam> _examCollecction;
        private readonly IMongoCollection<Subject> _subjectCollection;
        private readonly IMongoCollection<Student> _studentCollection;

        public MarksController(IMongoDatabase database)
        {
            _markCollection = database.GetCollection<Mark>("marks");
            _examCollecction = database.GetCollection<Exam>("exams");
            _subjectCollection = database.GetCollection<Subject>("subjects");
            _studentCollection = database.GetCollection<Student>("students");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mark>>> GetAllMarks()
        {
            var marks = await _markCollection.Find(m => true).ToListAsync();
            if(marks.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Marks not found" } });
            }

            var marksWithSubjectAndExamAndStudent = new List<Mark>();
            foreach(var mark in marks)
            {
                var subject = await _subjectCollection.Find(s => s.Id == mark.SubjectId).FirstOrDefaultAsync();
                var exam = await _examCollecction.Find(e => e.Id == mark.ExamId).FirstOrDefaultAsync();
                var student = await _studentCollection.Find(st => st.Id == mark.StudentId).FirstOrDefaultAsync();
                var markWithSubjectAndExamAndStudent = new Mark
                {
                    Id = mark.Id,
                    SubjectId = mark.SubjectId,
                    Subjects = subject,
                    ExamId = mark.ExamId,
                    Exams = exam,
                    StudentId = mark.StudentId,
                    Students = student
                };

                marksWithSubjectAndExamAndStudent.Add(markWithSubjectAndExamAndStudent);
            }
            return Ok( new { data = new { success = true, marks = marksWithSubjectAndExamAndStudent } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Mark>> GetMarkById(string id)
        {
            var mark = await _markCollection.Find(m => m.Id == id).FirstOrDefaultAsync();
            if (mark == null)
            {
                return NotFound(new { data = new { success = true, message = "This mark not available" } });
            }

            var subject = await _subjectCollection.Find(s => s.Id == mark.SubjectId).FirstOrDefaultAsync();
            var exam = await _examCollecction.Find(e => e.Id == mark.ExamId).FirstOrDefaultAsync();
            var student = await _studentCollection.Find(st => st.Id == mark.StudentId).FirstOrDefaultAsync();
            var markWithSubjectAndExamAndStudent = new Mark
            {
                Id = mark.Id,
                SubjectId = mark.SubjectId,
                Subjects = subject,
                ExamId = mark.ExamId,
                Exams = exam,
                StudentId = mark.StudentId,
                Students = student
            };

            return Ok(new { data = new { success = true, marks = markWithSubjectAndExamAndStudent } });
        }

        [HttpPost]
        public async Task<ActionResult<Mark>> CreateMark(Mark newMark)
        {
            var subject = await _subjectCollection.Find(s => s.Id == newMark.SubjectId).FirstOrDefaultAsync();
            if (subject == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid subject" } });
            }
            var exam = await _examCollecction.Find(e => e.Id == newMark.ExamId).FirstOrDefaultAsync();
            if (exam == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid exam" } });
            }
            var student = await _studentCollection.Find(st => st.Id == newMark.StudentId).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid student" } });
            }

            newMark.Subjects = subject;
            newMark.Exams = exam;
            newMark.Students = student;
            newMark.IsActive = true;
            newMark.CreatedAt = DateTime.UtcNow;
            await _markCollection.InsertOneAsync(newMark);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Mark created successfully...",
                    mark = new
                    {
                        id = newMark.Id,
                        subject = new
                        {
                            id = subject.Id,
                            name = subject.Name,
                        },
                        exam = new
                        {
                            id = exam.Id,
                            name = exam.Name,
                        },
                        student = new
                        {
                            id = student.Id,
                            firstName = student.FirstName,
                            lastName = student.LastName,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMark(string id, Mark updateMark)
        {
            var existingMark = await _markCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Mark>.Update
                .Set(a => a.StudentId, updateMark.StudentId)
                .Set(a => a.SubjectId, updateMark.SubjectId)
                .Set(a => a.ExamId, updateMark.ExamId));
            if (existingMark == null)
            {
                return NotFound(new { data = new { success = true, message = "This mark not available" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Mark updated successfully...",
                    mark = updateMark
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarks(string id)
        {
            var result = await _markCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This marks is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Marks deleted successfully..." } });
        }
    }
}

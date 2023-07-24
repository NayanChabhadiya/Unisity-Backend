using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly IMongoCollection<Subject> _subjectCollection;
        private readonly IMongoCollection<Course> _courseCollection;

        public SubjectsController(IMongoDatabase database)
        {
            _subjectCollection = database.GetCollection<Subject>("subjects");
            _courseCollection = database.GetCollection<Course>("courses");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subject>>> GetAllExam()
        {
            var subjects = await _subjectCollection.Find(s => true).ToListAsync();
            if (subjects.Count == 0)
            {
                return NotFound(new { data = new { success = true, message = "Subjects not found" } });
            }

            var subjectsWithCourse = new List<Subject>();
            foreach (var subjcet in subjects)
            {
                var cource = await _courseCollection.Find(c => c.Id == subjcet.CourceId).FirstOrDefaultAsync();
                var subjectWithCourse = new Subject
                {
                    Id = subjcet.Id,
                    Name = subjcet.Name,
                    CourceId = subjcet.CourceId,
                    Course = cource,
                };
                subjectsWithCourse.Add(subjectWithCourse);
            }

            return Ok(new { data = new { success = true, subjects = subjectsWithCourse } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Subject>> GetExamById(string id)
        {
            var subject = await _subjectCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (subject == null)
            {
                return NotFound(new { data = new { success = false, message = "This subject was not found" } });
            }

            var cource = await _courseCollection.Find(c => c.Id == subject.CourceId).FirstOrDefaultAsync();
            var subjectWithCource = new Subject
            {
                Id = subject.Id,
                Name = subject.Name,
                CourceId = subject.CourceId,
                Course = cource,
            };

            return Ok(new { data = new { success = true, subject = subjectWithCource } });
        }

        [HttpPost]
        public async Task<ActionResult<Subject>> CreateExam(Subject newSubject)
        {
            var cource = await _courseCollection.Find(c => c.Id == newSubject.CourceId).FirstOrDefaultAsync();
            if (cource == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid course" } });
            }

            var subjectExists = await _subjectCollection.Find(a => a.Name == newSubject.Name).FirstOrDefaultAsync();
            if (subjectExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Subject already exists" } });
            }

            newSubject.Course = cource;
            newSubject.IsActive = true;
            newSubject.CreatedAt = DateTime.UtcNow;
            await _subjectCollection.InsertOneAsync(newSubject);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Exam created successfully...",
                    subject = new
                    {
                        id = newSubject.Id,
                        name = newSubject.Name,
                        cource = new
                        {
                            id = cource.Id,
                            name = cource.Name,
                            description = cource.Description,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(string id, Subject updateSubject)
        {
            var existingSubject = await _subjectCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Subject>.Update
                .Set(a => a.Name, updateSubject.Name)
                .Set(a => a.CourceId, updateSubject.CourceId));
            if (existingSubject == null)
            {
                return NotFound(new { data = new { success = false, message = "This subject was not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Subject updated successfully...",
                    subject = updateSubject
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(string id)
        {
            var result = await _subjectCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This subject is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Subject deleted successfully..." } });
        }
    }
}

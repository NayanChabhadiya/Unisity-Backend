using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IMongoCollection<Exam> _examCollection;
        private readonly IMongoCollection<Course> _courseCollection;

        public ExamsController(IMongoDatabase database)
        {
            _examCollection = database.GetCollection<Exam>("exams");
            _courseCollection = database.GetCollection<Course>("cources");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exam>>> GetAllExam()
        {
            var exams = await _examCollection.Find(e => true).ToListAsync();
            if(exams.Count == 0)
            {
                return NotFound(new { data = new { success = true, message = "Exams not found" } });
            }

            var examsWithCource = new List<Exam>();
            foreach (var exam in exams)
            {
                var cource = await _courseCollection.Find(c => c.Id == exam.CourceId).FirstOrDefaultAsync();
                var examWithCource = new Exam
                {
                    Id = exam.Id,
                    Name = exam.Name,
                    CourceId = exam.CourceId,
                    Course = cource,
                };
                examsWithCource.Add(examWithCource);
            }

            return Ok(new { data = new {success = true, exams = examsWithCource} });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Exam>> GetExamById(string id)
        {
            var exam = await _examCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (exam == null) 
            { 
                return NotFound(new { data = new { success = false, message = "This exam was not found" } }); 
            }

            var course = await _courseCollection.Find(c => c.Id == exam.CourceId).FirstOrDefaultAsync();
            var examWithCource = new Exam
            {
                Id = exam.Id,
                Name = exam.Name,
                CourceId = exam.CourceId,
                Course = course,
            };

            return Ok(new { data = new { success = true, exam = examWithCource } });
        }

        [HttpPost]
        public async Task<ActionResult<Exam>> CreateExam(Exam newExam)
        {
            var course = await _courseCollection.Find(c => c.Id == newExam.CourceId).FirstOrDefaultAsync();
            if (course == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid course" } });
            }

            var examExists = await _examCollection.Find(a => a.Name == newExam.Name).FirstOrDefaultAsync();
            if (examExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Exam already exists" } });
            }

            newExam.Course = course;
            newExam.IsActive = true;
            newExam.CreatedAt = DateTime.UtcNow;
            await _examCollection.InsertOneAsync(newExam);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Exam created successfully...",
                    exam = new
                    {
                        id = newExam.Id,
                        name = newExam.Name,
                        cource = new
                        {
                            id = course.Id,
                            name = course.Name,
                            description = course.Description,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExam(string id, Exam updaetExam)
        {
            var existingExam = await _examCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Exam>.Update
                .Set(a => a.Name, updaetExam.Name)
                .Set(a => a.CourceId, updaetExam.CourceId));
            if (existingExam == null)
            {
                return NotFound(new { data = new { success = false, message = "This exam was not found" } });
            }
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Exam updated successfully...",
                    exam = updaetExam
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(string id)
        {
            var result = await _examCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This exam is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Exam deleted successfully..." } });
        }
    }
}

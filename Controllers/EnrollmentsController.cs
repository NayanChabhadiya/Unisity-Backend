using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IMongoCollection<Enrollment> _enrollmentCollection;
        private readonly IMongoCollection<Course> _courseCollection;
        private readonly IMongoCollection<Student> _studentCollection;

        public EnrollmentsController(IMongoDatabase database)
        {
            _enrollmentCollection = database.GetCollection<Enrollment>("enrollements");
            _courseCollection = database.GetCollection<Course>("courses");
            _studentCollection = database.GetCollection<Student>("students");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetAllEnrollments()
        {
            var enrollments = await _enrollmentCollection.Find(e => true).ToListAsync();
            if(enrollments.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Enrollments not found" } });
            }

            var enrollmentsWithCourseAndStudent = new List<Enrollment>();
            foreach(var enrollment in enrollments)
            {
                var course = await _courseCollection.Find(c => c.Id == enrollment.CourceId).FirstOrDefaultAsync();
                var student = await _studentCollection.Find(s => s.Id == enrollment.StudentId).FirstOrDefaultAsync();
                var enrollmentWithCourseAndStudent = new Enrollment
                {
                    Id = enrollment.CourceId,
                    Grade = enrollment.Grade,
                    CourceId = enrollment.CourceId,
                    Course = course,
                    StudentId = enrollment.StudentId,
                    Student = student,
                };
                enrollmentsWithCourseAndStudent.Add(enrollmentWithCourseAndStudent);
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    enrollments = enrollmentsWithCourseAndStudent
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollmentById(string id)
        {
            var enrollment = await _enrollmentCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if(enrollment == null)
            {
                return NotFound(new { data = new { success = false, message = "This enrollment was not found" } });
            }
            var course = await _courseCollection.Find(c => c.Id == enrollment.CourceId).FirstOrDefaultAsync();
            var student = await _studentCollection.Find(s => s.Id == enrollment.StudentId).FirstOrDefaultAsync();
            var enrollmentWithCourseAndStudent = new Enrollment
            {
                Id = enrollment.Id,
                Grade = enrollment.Grade,
                CourceId = enrollment.CourceId,
                Course = course,
                StudentId = enrollment.StudentId,
                Student = student,
            };

            return Ok( new {data = new {success = true, enrollment =  enrollmentWithCourseAndStudent} });
        }

        [HttpPost]
        public async Task<ActionResult<Enrollment>> CreateEnrollment(Enrollment newEnrollment)
        {
            var course = await _courseCollection.Find(c => c.Id == newEnrollment.CourceId).FirstOrDefaultAsync();
            if(course == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid course" } });
            }
            var student = await _studentCollection.Find(s => s.Id == newEnrollment.StudentId).FirstOrDefaultAsync();
            if(student == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid student" } });
            }

            newEnrollment.Course = course;
            newEnrollment.Student = student;
            newEnrollment.IsActive = true;
            newEnrollment.CreatedAt = DateTime.UtcNow;
            await _enrollmentCollection.InsertOneAsync(newEnrollment);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Enrollment created successfully...",
                    enrollment = new
                    {
                        id = newEnrollment.Id,
                        grade = newEnrollment.Grade,
                        cource = new
                        {
                            id = course.Id,
                            name = course.Name
                        },
                        student = new
                        {
                            id = student.Id,
                            firstName = student.FirstName,
                            lastName = student.LastName,
                            email = student.Email
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollment(string id, Enrollment updateEnrollment)
        {
            var existingEnrollment = await _enrollmentCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Enrollment>.Update
                .Set(a => a.Grade, updateEnrollment.Grade)
                .Set(a => a.CourceId, updateEnrollment.CourceId)
                .Set(a => a.StudentId, updateEnrollment.StudentId));
            if (existingEnrollment == null)
            {
                return NotFound(new { data = new { success = false, message = "This enrollment was not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Enrollment updated successfully...",
                    enrollment = updateEnrollment
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(string id)
        {
            var result = await _enrollmentCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This enrollment is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Enrollment deleted successfully..." } });
        }
    }
}

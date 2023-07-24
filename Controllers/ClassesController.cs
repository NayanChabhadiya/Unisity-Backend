using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly IMongoCollection<Class> _classCollection;
        private readonly IMongoCollection<Course> _courseCollection;
        private readonly IMongoCollection<Faculty> _facultyCollection;

        public ClassesController(IMongoDatabase database)
        {
            _classCollection = database.GetCollection<Class>("classes");
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _courseCollection = database.GetCollection<Course>("courses");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Class>>> GetAllClasses()
        {
            var classes = await _classCollection.Find(c => true).ToListAsync();
            if(classes.Count == 0)
            {
                return NotFound(new {data = new { success = false, message = "Classes not found"}});
            }
            var classesWithFacultyAndCourse = new List<Class>();
            foreach (var c in classes)
            {
                var faculty = await _facultyCollection.Find(f => f.Id == c.FacultyId).FirstOrDefaultAsync();
                var course = await _courseCollection.Find(cr => cr.Id == c.CourseId).FirstOrDefaultAsync();
                var classWithFacultyAndCourse = new Class
                {
                    Id = c.CourseId,
                    Name = c.Name,
                    Division = c.Division,
                    No = c.No,
                    CourseId = c.CourseId,
                    Course = course,
                    FacultyId = c.FacultyId,
                    Faculty = faculty,
                };
                classesWithFacultyAndCourse.Add(classWithFacultyAndCourse);
            }

            return Ok(new { data = new { success = true, classes = classesWithFacultyAndCourse } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Class>> GetClassById(string id)
        {
            var classes = await _classCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (classes == null)
            {
                return NotFound(new { data = new { success = false, message = "This Class is not found" } });
            }

            var faculty = await _facultyCollection.Find(f => f.Id == classes.FacultyId).FirstOrDefaultAsync();
            var course = await _courseCollection.Find(c => c.Id == classes.CourseId).FirstOrDefaultAsync();
            var classWithFacultyAndCOurse = new Class
            {
                Id = classes.Id,
                Name = classes.Name,
                Division = classes.Division,
                No = classes.No,
                FacultyId = classes.FacultyId,
                Faculty = faculty,
                CourseId = classes.CourseId,
                Course = course,
            };

            return Ok(new {data = new {success = true, classes =  classWithFacultyAndCOurse} });
        }

        [HttpPost]
        public async Task<ActionResult<Class>> CreateClass(Class newClass)
        {
            var faculty = await _facultyCollection.Find(f => f.Id == newClass.FacultyId).FirstOrDefaultAsync();
            if (faculty == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid faculty" } });
            }
            var course = await _courseCollection.Find(c => c.Id == newClass.CourseId).FirstOrDefaultAsync();
            if(course == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid course" } });
            }

            var classExists = await _classCollection.Find(a => a.Name == newClass.Name).FirstOrDefaultAsync();
            if (classExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Class name already exists" } });
            }

            var classNumberExists = await _classCollection.Find(a => a.No == newClass.No).FirstOrDefaultAsync();
            if (classNumberExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Class no already exists" } });
            }

            newClass.Course = course;
            newClass.Faculty = faculty;
            newClass.IsActive = true;
            newClass.CreatedAt = DateTime.UtcNow;
            await _classCollection.InsertOneAsync(newClass);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Class created successfully...",
                    classes = new
                    {
                        id = newClass.Id,
                        name = newClass.Name,
                        division = newClass.Division,
                        no = newClass.No,
                        faculty = new
                        {
                            id = faculty.Id,
                            firstName = faculty.FirstName,
                            lastName = faculty.LastName,
                            email = faculty.Email,
                        },
                        course = new
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
        public async Task<IActionResult> UpdateClass(string id,  Class updateClass)
        {
            var existingClass = await _classCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Class>.Update
                .Set(a => a.Name, updateClass.Name)
                .Set(a => a.Division, updateClass.Division)
                .Set(a => a.No, updateClass.No)
                .Set(a => a.FacultyId, updateClass.FacultyId)
                .Set(a => a.CourseId, updateClass.CourseId));
            if (existingClass == null )
            {
                return NotFound(new { data = new { success = false, message = "This Class is not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Class updated successfully...",
                    classes = updateClass
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(string id)
        {
            var result = await _classCollection.DeleteOneAsync(c => c.Id == id);
            if(result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This Class is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Class deleted successfully..." } });
        }
    }
}

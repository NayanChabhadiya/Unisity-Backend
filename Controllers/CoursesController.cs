using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IMongoCollection<Course> _courseCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;

        public CoursesController(IMongoDatabase database)
        {
            _courseCollection = database.GetCollection<Course>("Courses");
            _organizationCollection = database.GetCollection<Organization>("organizations");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCourse()
        {
            var courses = await _courseCollection.Find(c => true).ToListAsync();
            if(courses.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Course not found" } });
            }

            var coursesWithOrganization = new List<Course>();
            foreach (var course in courses)
            {
                var organization = await _organizationCollection.Find(o => o.Id == course.OrganizationId).FirstOrDefaultAsync();
                var courseWithOrganization = new Course
                {
                    Id = course.Id,
                    Name = course.Name,
                    Description = course.Description,
                    OrganizationId = course.OrganizationId,
                    Organization = organization,
                };

                coursesWithOrganization.Add(courseWithOrganization);
            }

            return Ok( new {data = new {success = true, courses = coursesWithOrganization} });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetcourseById(string id)
        {
            var course = await _courseCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (course == null)
            {
                return NotFound(new { data = new { success = false, message = "This course was not found" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == course.OrganizationId).FirstOrDefaultAsync();
            var courseWithOrganization = new Course
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                OrganizationId = course.OrganizationId,
                Organization = organization,
            };

            return Ok( new {data = new { success = true, course = courseWithOrganization} });
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course newCourse)
        {
            var organization =  await _organizationCollection.Find(o => o.Id == newCourse.OrganizationId).FirstOrDefaultAsync();
            if(organization == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid organization" } });
            }

            var courseExists = await _courseCollection.Find(a => a.Name == newCourse.Name && a.OrganizationId == organization.Id).FirstOrDefaultAsync();
            if (courseExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "This organization have already exists this course" } });
            }

            newCourse.Organization = organization;
            newCourse.IsActive = true;
            newCourse.CreatedAt = DateTime.UtcNow;
            await _courseCollection.InsertOneAsync(newCourse);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Course created successfully",
                    course = new
                    {
                        id = newCourse.Id,
                        name = newCourse.Name,
                        description = newCourse.Description,
                        organization = new
                        {
                            id = organization.Id,
                            name = organization.Name,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Updatecourse(string id, Course updatecourse)
        {
            var existingcourse = await _courseCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Course>.Update
                .Set(a => a.Name, updatecourse.Name)
                .Set(a => a.Description, updatecourse.Description)
                .Set(a => a.OrganizationId, updatecourse.OrganizationId));
            if (existingcourse == null)
            {
                return NotFound(new { data = new { success = false, message = "This course was not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Course updated successfully",
                    course = updatecourse
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletecourses(string id)
        {
            var result = await _courseCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This course is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Course deleted successfully..." } });
        }
    }
}

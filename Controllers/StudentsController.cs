using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IMongoCollection<Student> _studentCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public StudentsController(IMongoDatabase database)
        {
            _studentCollection = database.GetCollection<Student>("students");
            _organizationCollection = database.GetCollection<Organization>("organizations");
            _roleCollection = database.GetCollection<Role>("roles");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllFaculties()
        {
            var students = await _studentCollection.Find(s => true).ToListAsync();
            if (students.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Students Not Found" } });
            }

            var studentsWithOrganizationAndRole = new List<Student>();
            foreach (var student in students)
            {
                var organization = await _organizationCollection.Find(o => o.Id == student.OrganizationId).FirstOrDefaultAsync();
                var role = await _roleCollection.Find(r => r.Id == student.RoleId).FirstOrDefaultAsync();
                var studentWithOrganizationAndRole = new Student
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    PasswordHash = student.PasswordHash,
                    OrganizationId = student.OrganizationId,
                    Organizations = organization,
                    RoleId = student.RoleId,
                    Roles = role
                };
                studentsWithOrganizationAndRole.Add(studentWithOrganizationAndRole);
            }

            return Ok(new { data = new { success = true, students = students, roleName = students[0].Roles.Name } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudentById(string id)
        {
            var student = await _studentCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound(new { data = new { success = false, message = "This student is not found" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == student.OrganizationId).FirstOrDefaultAsync();
            var role = await _roleCollection.Find(r => r.Id == student.RoleId).FirstOrDefaultAsync();
            var studentWithOrganizationAndRole = new Student
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PasswordHash = student.PasswordHash,
                OrganizationId = student.OrganizationId,
                Organizations = organization,
                RoleId = student.RoleId,
                Roles = role
            };

            return Ok(new { data = new { success = false, student = studentWithOrganizationAndRole, roleName = student.Roles.Name } });
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student newStudent)
        {
            var role = await _roleCollection.Find(r => r.Id == newStudent.RoleId).FirstOrDefaultAsync();
            if (role == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid role" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == newStudent.OrganizationId).FirstOrDefaultAsync();
            if (organization == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid organization" } });
            }

            var studentExists = await _studentCollection.Find(a => a.Email == newStudent.Email).FirstOrDefaultAsync();
            if (studentExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Email already exists" } });
            }

            newStudent.Roles = role;
            newStudent.IsActive = true;
            newStudent.Organizations = organization;
            newStudent.PasswordHash = HashPassword(newStudent.PasswordHash);
            newStudent.CreatedAt = DateTime.UtcNow;

            await _studentCollection.InsertOneAsync(newStudent);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Student created successfully...",
                    student = new
                    {
                        id = newStudent.Id,
                        firstName = newStudent.FirstName,
                        lastName = newStudent.LastName,
                        email = newStudent.Email,
                        organization = new
                        {
                            id = organization.Id,
                            name = organization.Name,
                        }
                    }
                }
            });

        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(string id, Student updateStudent)
        {
            var existingStudent = await _studentCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Student>.Update
                .Set(a => a.FirstName, updateStudent.FirstName)
                .Set(a => a.LastName, updateStudent.LastName)
                .Set(a => a.Email, updateStudent.Email)
                .Set(a => a.PasswordHash, updateStudent.PasswordHash)
                .Set(a => a.RoleId, updateStudent.RoleId)
                .Set(a => a.OrganizationId, updateStudent.OrganizationId));
            if(existingStudent == null)
            {
                return NotFound(new { data = new { success = false, message = "This student is not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Student updaetd successfully...",
                    student = updateStudent
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var result = await _studentCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This student is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Student deleted successfully..." } });
        }
    }
}

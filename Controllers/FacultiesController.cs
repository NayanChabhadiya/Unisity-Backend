using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultiesController : ControllerBase
    {
        private readonly IMongoCollection<Faculty> _facultyCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public FacultiesController(IMongoDatabase database)
        {
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _organizationCollection = database.GetCollection<Organization>("organizations");
            _roleCollection = database.GetCollection<Role>("roles");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Faculty>>> GetAllFaculties()
        {
            var faculties = await _facultyCollection.Find(f => true).ToListAsync();
            if(faculties.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Faculties Not Found" } });
            }

            var facultiesWithOrganizationAndRole = new List<Faculty>();
            foreach(var faculty in faculties)
            {
                var organization = await _organizationCollection.Find(o => o.Id == faculty.OrganizationId).FirstOrDefaultAsync();
                var role = await _roleCollection.Find(r => r.Id == faculty.RoleId).FirstOrDefaultAsync();
                var facultyWithOrganizationAndRole = new Faculty
                {
                    Id = faculty.Id,
                    FirstName = faculty.FirstName,
                    LastName = faculty.LastName,
                    Email = faculty.Email,
                    PasswordHash = faculty.PasswordHash,
                    OrganizationId = faculty.OrganizationId,
                    Organizations = organization,
                    RoleId = faculty.RoleId,
                    Roles = role
                };
                facultiesWithOrganizationAndRole.Add(facultyWithOrganizationAndRole);
            }

            return Ok(new { data = new { success = true, faculties = faculties, roleName = faculties[0].Roles.Name } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Faculty>> GetFacultyById(string id)
        {
            var faculty = await _facultyCollection.Find(f => f.Id == id).FirstOrDefaultAsync();
            if(faculty == null)
            {
                return NotFound(new { data = new { success = false, message = "This faculty is not found" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == faculty.OrganizationId).FirstOrDefaultAsync();
            var role = await _roleCollection.Find(r => r.Id == faculty.RoleId).FirstOrDefaultAsync();
            var facultyWithOrganizationAndRole = new Faculty
            {
                Id = faculty.Id,
                FirstName = faculty.FirstName,
                LastName = faculty.LastName,
                Email = faculty.Email,
                PasswordHash = faculty.PasswordHash,
                OrganizationId = faculty.OrganizationId,
                Organizations = organization,
                RoleId = faculty.RoleId,
                Roles = role
            };

            return Ok(new {data = new {success = true, faculty = facultyWithOrganizationAndRole, roleName = faculty.Roles.Name}});
        }

        [HttpPost]
        public async Task<ActionResult<Faculty>> CreateFaculty(Faculty newFaculty)
        {
            var role = await _roleCollection.Find(r => r.Id == newFaculty.RoleId).FirstOrDefaultAsync();
            if(role == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid role" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == newFaculty.OrganizationId).FirstOrDefaultAsync();
            if (organization == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid organization" } });
            }

            var facultyExists = await _facultyCollection.Find(a => a.Email == newFaculty.Email).FirstOrDefaultAsync();
            if (facultyExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Email already exists" } });
            }

            newFaculty.Roles = role;
            newFaculty.Organizations = organization;
            newFaculty.IsActive = true;
            newFaculty.CreatedAt = DateTime.UtcNow;
            newFaculty.PasswordHash = HashPassword(newFaculty.PasswordHash);

            await _facultyCollection.InsertOneAsync(newFaculty);
            return Ok(new {data = new {success = true, message = "Faculty created successfully...", faculty = newFaculty}});

        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFaculty(string id,Faculty updateFaculty)
        {
            var existingFaculty = await _facultyCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Faculty>.Update
                .Set(a => a.FirstName, updateFaculty.FirstName)
                .Set(a => a.LastName, updateFaculty.LastName)
                .Set(a => a.Email, updateFaculty.Email)
                .Set(a => a.PasswordHash, updateFaculty.PasswordHash)
                .Set(a => a.OrganizationId, updateFaculty.OrganizationId)
                .Set(a => a.RoleId, updateFaculty.RoleId));
            if(existingFaculty == null)
            {
                return NotFound(new { data = new { success = false, message = "This faculty is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Faculty updated successfully...", faculty = updateFaculty } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaculty(string id)
        {
            var result = await _facultyCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This faculty is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Faculty deleted successfully..." } });
        }


    }
}

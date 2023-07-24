using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;
        private readonly IMongoCollection<Faculty> _facultyCollection;
        private readonly IMongoCollection<Student> _studentCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public UsersController(IMongoDatabase database)
        {
            _roleCollection = database.GetCollection<Role>("roles");
            _adminCollection = database.GetCollection<Admin>("admins");
            _organizationCollection = database.GetCollection<Organization>("organizations");
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _studentCollection = database.GetCollection<Student>("students");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var admins = await _adminCollection.Find(a => true).ToListAsync();
            if(admins.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Admins not found"}});
            }
            var organizations = await _organizationCollection.Find(o => true).ToListAsync();
            if (organizations.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Organizations not found" } });
            }
            var faculties = await _facultyCollection.Find(f => true).ToListAsync();
            if (faculties.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Faculties not found" } });
            }
            var students = await _studentCollection.Find(s => true).ToListAsync();
            if (students.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Students not found" } });
            }

            return Ok(new { 
                data = new 
                { 
                    success = true, 
                    admins = admins,
                    organizations = organizations,
                    faculties = faculties,
                    students = students
                } 
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFacultiesAndStudentsByOrganization(string id)
        {
            //var organization = await _organizationCollection.Find(o => id == o.Id).FirstOrDefaultAsync();
            //if (organization == null)
            //{
            //    return NotFound(new { data = new { success = false, message = "This organization not found" } });
            //}

            var faculties = await _facultyCollection.Find(f => id == f.OrganizationId).ToListAsync();
            
            var students = await _studentCollection.Find(s => id == s.OrganizationId).ToListAsync();
            


            return Ok(new
            {
                data = new
                {
                    faculties = faculties,
                    students = students
                }
            });
        }
    }
}

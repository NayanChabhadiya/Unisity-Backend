using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IMongoCollection<Department> _departmentsCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;

        public DepartmentsController(IMongoDatabase database)
        {
            _departmentsCollection = database.GetCollection<Department>("departments");
            _organizationCollection = database.GetCollection<Organization>("organizations");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentsCollection.Find(d => true).ToListAsync();
            if(departments.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Department Was not found"}});
            }

            var departmentsWithOrganization = new List<Department>();
            foreach(var department in departments)
            {
                var organization = await _organizationCollection.Find(o => o.Id == department.OrganizationId).FirstOrDefaultAsync();
                var departmentWithRole = new Department
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    OrganizationId = department.OrganizationId,
                    Organizations = organization
                };


                departmentsWithOrganization.Add(departmentWithRole);
            }
            return Ok(new { data = new { success = true,department = departmentsWithOrganization } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartmentById(string id)
        {
            var department = await _departmentsCollection.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (department == null)
            {
                return NotFound(new { data = new { sucess = false, message = "This department is not available" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == department.OrganizationId).FirstOrDefaultAsync();
            if (organization == null)
            {
                return NotFound(new {data = new {success = true, message = "Invalid organization"}});
            }
            var departmentWithOrganization = new Department
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                OrganizationId = department.OrganizationId,
                Organizations = organization
            };

            return Ok(new { data = new { success = true, department = departmentWithOrganization } });
        }

        [HttpPost]
        public async Task<ActionResult<Department>> CreateDepartment(Department newDepartment)
        {
            var organization = await _organizationCollection.Find(o => o.Id == newDepartment.OrganizationId).FirstOrDefaultAsync();
            if(organization == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid organization" } });
            }


            var departmentExists = await _departmentsCollection.Find(a => a.Name == newDepartment.Name).FirstOrDefaultAsync();
            if (departmentExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Department already exists" } });
            }

            newDepartment.Organizations = organization;
            newDepartment.IsActive = true;
            newDepartment.CreatedAt = DateTime.UtcNow;
            await _departmentsCollection.InsertOneAsync(newDepartment);
            return Ok(new { data = new { success = true, Message = "Department created successfully....", department = newDepartment } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepratment(string id, Department updateDepartment)
        {
            var existingDepartment = await _departmentsCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Department>.Update
                .Set(a => a.Name, updateDepartment.Name)
                .Set(a => a.Description, updateDepartment.Description)
                .Set(a => a.OrganizationId, updateDepartment.OrganizationId));
            if(existingDepartment == null)
            {
                return NotFound(new { data = new { sucess = false, message = "This department is not available" } });
            }

            return Ok(new { data = new { success = true, Message = "Department updated successfully....", department = updateDepartment } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(string id)
        {
            var result = await _departmentsCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This department is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Department deleted successfully..." } });
        }

    }
}

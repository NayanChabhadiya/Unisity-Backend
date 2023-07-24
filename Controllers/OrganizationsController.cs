using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMongoCollection<Organization> _organizationCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public OrganizationsController(IMongoDatabase database)
        {
            _organizationCollection = database.GetCollection<Organization>("organizations");
            _roleCollection = database.GetCollection<Role>("roles");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organization>>> GetAllOrganizatons()
        {
            var organizations = await _organizationCollection.Find(o => true).ToListAsync();
            if (organizations.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Organization not found" } });
            }

            var organizationsWithRole = new List<Organization>();

            foreach (var organization in organizations)
            {
                var role = await _roleCollection.Find(o => o.Id == organization.RoleId).FirstOrDefaultAsync();
                var orgWithRole = new Organization
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    Email = organization.Email,
                    PasswordHash = organization.PasswordHash,
                    RoleId = organization.RoleId,
                    Roles = role,
                };
                organizationsWithRole.Add(orgWithRole);
            }
            return Ok(new { data = new { success = true, organizations = organizations, roleName = organizations[0].Roles.Name } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganizationById(string id)
        {
            var organization = await _organizationCollection.Find(o =>  id == o.Id).FirstOrDefaultAsync();
            if (organization == null)
            {
                return NotFound(new {data = new { success = false, message = "This organization not found" } });
            }

            var role = await _roleCollection.Find(o => o.Id == organization.RoleId).FirstOrDefaultAsync();
            var organizatinWithRole = new Organization
            {
                Id = organization.Id,
                Name = organization.Name,
                Email = organization.Email,
                PasswordHash = organization.PasswordHash,
                RoleId = organization.RoleId,
                Roles = role,
            };

            return Ok(new { data = new { success = true, organization = organization, roleName = organization.Roles.Name } });
        }

        [HttpPost]
        public async Task<ActionResult<Organization>> CreateOrganization(Organization newOrganization)
        {
            var role = await _roleCollection.Find(r => r.Id == newOrganization.RoleId).FirstOrDefaultAsync();
            if (role == null)
            {
                return NotFound(new { data = new {success = false, message = "Invalid role" } });
            }

            var organizationExists = await _organizationCollection.Find(a => a.Email == newOrganization.Email).FirstOrDefaultAsync();
            if (organizationExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Email already exists" } });
            }

            newOrganization.PasswordHash = HashPassword(newOrganization.PasswordHash);
            newOrganization.Roles = role;
            newOrganization.IsActive = true;
            newOrganization.CreatedAt = DateTime.UtcNow;
            await _organizationCollection.InsertOneAsync(newOrganization);
            return Ok(new { data = new { success = true, message = "Organization created successfully...", organization = newOrganization } });
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(string id, Organization updateOrganization)
        {
            var existingOrganization = await _organizationCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Organization>.Update
                .Set(a => a.Email, updateOrganization.Email)
                .Set(a => a.Name, updateOrganization.Name)
                .Set(a => a.RoleId, updateOrganization.RoleId));
            if(existingOrganization == null)
            {
                return NotFound(new { data = new { success = false, message = "This organization not found" } });
            }

            return Ok(new { data = new { success = true, message = "Organization updated successfully...", organization = updateOrganization } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            var result = await _organizationCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This organization is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Organization deleted successfully..." } });
        }
    }
}

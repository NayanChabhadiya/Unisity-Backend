using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IMongoCollection<Role> _roleCollection;

        public RolesController(IMongoDatabase database)
        {
            _roleCollection = database.GetCollection<Role>("roles");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            var roles = await _roleCollection.Find(r => true).ToListAsync();
            if(roles.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Role not found" } });
            }
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(string id)
        {
            var role = await _roleCollection.Find(r => r.Id == id).FirstOrDefaultAsync(); 
            if(role == null)
            {
                return NotFound(new { data = new { success = false, message = "Role not found" } });
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(Role newRole)
        {
            var rollExists = await _roleCollection.Find(a => a.Name == newRole.Name).FirstOrDefaultAsync();
            if(rollExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Role already exists" } });
            }
            newRole.CreatedAt = DateTime.UtcNow;
            await _roleCollection.InsertOneAsync(newRole);
            return Ok(new { data = new { success = true, message = "Role created successfully.....", role = newRole } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, Role updateRole)
        {
            var existingRole = await _roleCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Role>.Update
                .Set(a => a.Name, updateRole.Name));
            if(existingRole == null)
            {
                return NotFound(new { data = new { success = false, message = "Role not found" } });
            }

            return Ok(new { data = new { success = true, message = "Role updated successfully.....", role = updateRole } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var result = await _roleCollection.DeleteOneAsync(r => r.Id == id);
            if(result.DeletedCount == 0)
            {
                return NotFound(new { data = new { message = "Role not found" } });
            }

            return Ok(new { data = new { sucess = true, message = "Role successfully deleted" } });
        }
    }
}

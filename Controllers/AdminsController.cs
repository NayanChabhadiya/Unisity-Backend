using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public AdminsController(IMongoDatabase database)
        {
            _adminCollection = database.GetCollection<Admin>("admins");
            _roleCollection = database.GetCollection<Role>("roles");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminCollection.Find(a => true).ToListAsync();
            if(admins.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Admins not found" } });
            }

            return Ok(new { data = new { success = true, admins = admins, roleName = admins[0].Roles.Name }});
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdminById(string id)
        {
            var admin = await _adminCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
            if(admin == null)
            {
                return NotFound(new { data = new { success = false, message = "This admin not found" } });
            }

            var role = await _roleCollection.Find(r => r.Id == admin.RoleId).FirstOrDefaultAsync();
            if(role == null)
            {
                return BadRequest(new { data = new { success = false, message = "Invalid role" } });
            }

            return Ok(new { data = new { success = true, admins = admin, role = role.Name } });
        }

        [HttpPost]
        public async Task<ActionResult<Admin>> CreateAdmin(Admin newAdmin)
        {
            var role = await _roleCollection.Find(r => r.Id == newAdmin.RoleId).FirstOrDefaultAsync();
            if (role == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid role" } });
            }

            var adminExists = await _adminCollection.Find(a => a.Email == newAdmin.Email).FirstOrDefaultAsync();
            if (adminExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Email already exists" } });
            }

            newAdmin.PasswordHash = HashPassword(newAdmin.PasswordHash);
            newAdmin.Roles = role;
            newAdmin.IsActive = true;
            newAdmin.CreatedAt = DateTime.UtcNow;
            await _adminCollection.InsertOneAsync(newAdmin);
            return Ok(new { data = new { success = true, message = "Admin created successfully....", admin = newAdmin } });
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(string id, Admin updateAdmin)
        {
            var existingAdmin = await _adminCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Admin>.Update
                .Set(a => a.FirstName, updateAdmin.FirstName)
                .Set(a => a.LastName, updateAdmin.LastName)
                .Set(a => a.Email, updateAdmin.Email)
                .Set(a => a.PasswordHash, updateAdmin.PasswordHash)
                .Set(a => a.RoleId, updateAdmin.RoleId));
            if (existingAdmin == null)
            {
                return NotFound(new { data = new { success = false, message = "This announcement not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Admin successfully updated...",
                    admin = updateAdmin
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var result = await _adminCollection.DeleteOneAsync(a => a.Id == id);
            if(result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This admin not found" } });
            }

            return Ok(new { data = new { success = true, message = "Admin deleted successfully...." } });
        }

    }
}

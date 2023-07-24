using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Role> _roleCollection;
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;
        private readonly IMongoCollection<Faculty> _facultyCollection;
        private readonly IMongoCollection<Student> _studentCollection;

        public AuthController(IMongoDatabase database, IConfiguration configuration)
        {
            _configuration = configuration;
            _roleCollection = database.GetCollection<Role>("roles");
            _adminCollection = database.GetCollection<Admin>("admins");
            _organizationCollection = database.GetCollection<Organization>("organizations");
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _studentCollection = database.GetCollection<Student>("students");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest loginRequest)
        {
            var admin = await _adminCollection.Find(a => a.Email == loginRequest.Email).FirstOrDefaultAsync();
            var organization = await _organizationCollection.Find(o => o.Email == loginRequest.Email).FirstOrDefaultAsync();
            var faculty = await _facultyCollection.Find(f => f.Email == loginRequest.Email).FirstOrDefaultAsync();
            var student = await _studentCollection.Find(f => f.Email == loginRequest.Email).FirstOrDefaultAsync();
            if (admin != null)
            {
                if(VerifyPassword(loginRequest.Password, admin.PasswordHash))
                {
                    var role = await _roleCollection.Find(r => r.Id == admin.RoleId).FirstOrDefaultAsync();
                    var token = GenerateJwtToken(admin.Email, role.Name);
                    return Ok(new
                    {
                        data = new
                        {
                            Success = true,
                            Message = "Login Successfully...",
                            AccessToken = token,
                            user = new
                            {
                                id = admin.Id,
                                firstName = admin.FirstName,
                                lastName = admin.LastName,
                                email = admin.Email,
                                role = role.Name
                            }
                        }
                    });
                }
                else
                {
                    return Unauthorized(new { data = new { Success = false, Message = "Invalid password" } });
                }
            }
            else if (organization != null)
            {
                if(VerifyPassword(loginRequest.Password, organization.PasswordHash))
                {
                    var role = await _roleCollection.Find(r => r.Id == organization.RoleId).FirstOrDefaultAsync();
                    var token = GenerateJwtToken(organization.Email, role.Name);

                    return Ok(new
                    {
                        data = new
                        {
                            success = true,
                            message = "Login successfully...",
                            accessToken = token,
                            user = new
                            {
                                id = organization.Id,
                                name = organization.Name,
                                email = organization.Email,
                                role = role.Name
                            }
                        }
                    });
                }
                else
                {
                    return Unauthorized(new { data = new { Success = false, Message = "Invalid password" } });
                }
            }
            else if (faculty != null)
            {
                if(VerifyPassword(loginRequest.Password, faculty.PasswordHash))
                {
                    var organizations = await _organizationCollection.Find(o => o.Id == faculty.OrganizationId).FirstOrDefaultAsync();
                    var role = await _roleCollection.Find(r => r.Id == faculty.RoleId).FirstOrDefaultAsync();
                    var token = GenerateJwtToken(faculty.Email, role.Name);

                    return Ok(new
                    {
                        data = new
                        {
                            success = true,
                            message = "Login successfully...",
                            accessToken = token,
                            user = new
                            {
                                id = faculty.Id,
                                firstName = faculty.FirstName,
                                lastName = faculty.LastName,
                                email = faculty.Email,
                                organization = new
                                {
                                    id = organizations.Id,
                                    name = organizations.Name,
                                },
                                role = role.Name
                            }
                        }
                    });
                }
                else
                {
                    return Unauthorized(new { data = new { Success = false, Message = "Invalid password" } });
                }
            }
            else if (student != null)
            {
                if (VerifyPassword(loginRequest.Password, student.PasswordHash))
                {
                    var organizations = await _organizationCollection.Find(o => o.Id == student.OrganizationId).FirstOrDefaultAsync();
                    var role = await _roleCollection.Find(r => r.Id == student.RoleId).FirstOrDefaultAsync();
                    var token = GenerateJwtToken(student.Email, role.Name);

                    return Ok(new
                    {
                        data = new
                        {
                            success = true,
                            message = "Login successfully...",
                            accessToken = token,
                            user = new
                            {
                                id = student.Id,
                                firstName = student.FirstName,
                                lastName = student.LastName,
                                email = student.Email,
                                organization = new
                                {
                                    id = organizations.Id,
                                    name = organizations.Name,
                                },
                                role = role.Name
                            }
                        }
                    });
                }
                else
                {
                    return Unauthorized(new { data = new { Success = false, Message = "Invalid password" } });
                }
            }
            else
            {
                return Unauthorized(new { data = new { Success = false, Message = "Invalid email address" } });
            }
        }

        [HttpPut("{id}/forgotAdminPassword")]
        public async Task<IActionResult> ForgotAdminPassword(string id, string newPassword)
        {
            var admin = await _adminCollection.Find(a => a.Id == id).FirstOrDefaultAsync(); 
            if (admin == null)
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid admin" } });
            }
            newPassword = HashPassword(newPassword);
            var existingPassword = await _adminCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Admin>.Update
                .Set(a => a.PasswordHash, newPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/changeAdminPassword")]
        public async Task<IActionResult> ChangeAdminPassword(string id, ChangePasswordRequest changePasswordRequest)
        {

            var admin = await _adminCollection.Find(a => a.Id == id).FirstOrDefaultAsync();

            if (!VerifyPassword(changePasswordRequest.OldPassword, admin.PasswordHash))
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid old password" } });
            }
            changePasswordRequest.NewPassword = HashPassword(changePasswordRequest.NewPassword);
            var existingPassword = await _adminCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Admin>.Update
                .Set(a => a.PasswordHash, changePasswordRequest.NewPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/forgotOrganizationPassword")]
        public async Task<IActionResult> ForgotOrganizationPassword(string id, string newPassword)
        {
            var organization = await _organizationCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (organization == null)
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid organization" } });
            }
            newPassword = HashPassword(newPassword);
            var existingPassword = await _organizationCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Organization>.Update
                .Set(a => a.PasswordHash, newPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }


        [HttpPut("{id}/changeOrganizationPassword")]
        public async Task<IActionResult> ChangeOrganizationPassword(string id, ChangePasswordRequest changePasswordRequest)
        {

            var organization = await _adminCollection.Find(a => a.Id == id).FirstOrDefaultAsync();

            if (!VerifyPassword(changePasswordRequest.OldPassword, organization.PasswordHash))
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid old password" } });
            }
            changePasswordRequest.NewPassword = HashPassword(changePasswordRequest.NewPassword);
            var existingPassword = await _organizationCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Organization>.Update
                .Set(a => a.PasswordHash, changePasswordRequest.NewPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/forgotFacultyPassword")]
        public async Task<IActionResult> ForgotFacultyPassword(string id, string newPassword)
        {
            var faculty = await _facultyCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (faculty == null)
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid faculty" } });
            }
            newPassword = HashPassword(newPassword);
            var existingPassword = await _facultyCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Faculty>.Update
                .Set(a => a.PasswordHash, newPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/changeFacultyPassword")]
        public async Task<IActionResult> ChangeFacultyPassword(string id, ChangePasswordRequest changePasswordRequest)
        {

            var faculty = await _facultyCollection.Find(a => a.Id == id).FirstOrDefaultAsync();

            if (!VerifyPassword(changePasswordRequest.OldPassword, faculty.PasswordHash))
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid old password" } });
            }
            changePasswordRequest.NewPassword = HashPassword(changePasswordRequest.NewPassword);
            var existingPassword = await _facultyCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Faculty>.Update
                .Set(a => a.PasswordHash, changePasswordRequest.NewPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/forgotStudentPassword")]
        public async Task<IActionResult> ForgotStudentPassword(string id, string newPassword)
        {
            var student = await _studentCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid student" } });
            }
            newPassword = HashPassword(newPassword);
            var existingPassword = await _studentCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Student>.Update
                .Set(a => a.PasswordHash, newPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }

        [HttpPut("{id}/changeStudentPassword")]
        public async Task<IActionResult> ChangeStudentPassword(string id, ChangePasswordRequest changePasswordRequest)
        {

            var student = await _studentCollection.Find(a => a.Id == id).FirstOrDefaultAsync();

            if (!VerifyPassword(changePasswordRequest.OldPassword, student.PasswordHash))
            {
                return BadRequest(new { data = new { Success = false, Message = "Invalid old password" } });
            }
            changePasswordRequest.NewPassword = HashPassword(changePasswordRequest.NewPassword);
            var existingPassword = await _studentCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Student>.Update
                .Set(a => a.PasswordHash, changePasswordRequest.NewPassword));
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Paasword changed successfully",
                }
            });
        }


        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateJwtToken(string username,string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialsController : ControllerBase
    {
        private readonly IMongoCollection<Material> _materialCollection;
        private readonly IMongoCollection<Subject> _subjectCollection;
        private readonly IMongoCollection<Faculty> _facultyCollectio;

        public MaterialsController(IMongoDatabase database)
        {
            _materialCollection = database.GetCollection<Material>("materials"); ;
            _subjectCollection = database.GetCollection<Subject>("subjects"); ;
            _facultyCollectio = database.GetCollection<Faculty>("faculties");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetAllMaterials()
        {
            var materials = await _materialCollection.Find(m => true).ToListAsync();
            if(materials.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Materials not found"}});
            }

            var materialsWithSubjectAndFaculty =  new List<Material>();
            foreach (var material in materials)
            {
                var subject = await _subjectCollection.Find(s => s.Id == material.SubjectId).FirstOrDefaultAsync();
                var faculty = await _facultyCollectio.Find(f => f.Id == material.FacultyId).FirstOrDefaultAsync();
                var materialWithSubjectAndFaculty = new Material
                {
                    Id = material.SubjectId,
                    SubjectId = material.SubjectId,
                    Subjects = subject,
                    FacultyId = material.FacultyId,
                    Faculties = faculty
                };
                materialsWithSubjectAndFaculty.Add(materialWithSubjectAndFaculty);
            }

            return Ok(new { data = new { success = true, materials = materialsWithSubjectAndFaculty } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetMaterialById(string id)
        {
            var material = await _materialCollection.Find(m => m.Id == id).FirstOrDefaultAsync();
            if(material == null)
            {
                return NotFound(new { data = new { success = false, message = "This material not found" } });
            }

            var subject = await _subjectCollection.Find(s => s.Id == material.SubjectId).FirstOrDefaultAsync();
            var faculty = await _facultyCollectio.Find(f => f.Id == material.FacultyId).FirstOrDefaultAsync();
            var materialWithSubjectAndFaculty = new Material
            {
                Id = material.SubjectId,
                SubjectId = material.SubjectId,
                Subjects = subject,
                FacultyId = material.FacultyId,
                Faculties = faculty
            };

            return Ok(new { data = new { success = true, material = materialWithSubjectAndFaculty } });
        }

        [HttpPost]
        public async Task<ActionResult<Material>> CreateMaterial(Material newMaterial)
        {
            var subject = await _subjectCollection.Find(s => s.Id == newMaterial.SubjectId).FirstOrDefaultAsync();
            if (subject == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid subject" } });
            }
            var faculty = await _facultyCollectio.Find(f => f.Id == newMaterial.FacultyId).FirstOrDefaultAsync();
            if(faculty == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid faculty" } });
            }

            newMaterial.Subjects = subject;
            newMaterial.Faculties = faculty;
            newMaterial.Faculties = faculty;
            newMaterial.CreatedAt = DateTime.UtcNow;
            await _materialCollection.InsertOneAsync(newMaterial);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Material created successfully",
                    material = new
                    {
                        id = newMaterial.Id,
                        faculty = new
                        {
                            id = faculty.Id,
                            firstName = faculty.FirstName,
                            lastName = faculty.LastName,
                            email = faculty.Email,
                        },
                        subject = new
                        {
                            id = subject.Id,
                            name = subject.Name,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaterial(string id, Material updateMaterial)
        {
            var existingMaterial = await _materialCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Material>.Update
                .Set(a => a.SubjectId, updateMaterial.SubjectId)
                .Set(a => a.FacultyId, updateMaterial.FacultyId));
            if (existingMaterial == null)
            {
                return NotFound(new { data = new { success = false, message = "This material not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Material updated successfully",
                    material = updateMaterial
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterial(string id)
        {
            var result = await _materialCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This material is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Material deleted successfully..." } });
        }
    }
}

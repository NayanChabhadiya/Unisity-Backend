using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IMongoCollection<Project> _projectCollection;
        private readonly IMongoCollection<Faculty> _facultyCollection;
        private readonly IMongoCollection<Student> _studentCollection;

        public ProjectsController(IMongoDatabase database)
        {
            _projectCollection = database.GetCollection<Project>("projects");
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _studentCollection = database.GetCollection<Student>("students");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
        {
            var projects = await _projectCollection.Find(p => true).ToListAsync();
            if(projects.Count == 0)
            {
                return NotFound(new { data =  new { success = false, message = "Projects not found"}});
            }

            var projetsWithFacultyAndStudent =  new List<Project>();
            foreach(var project in projects)
            {
                var faculty = await _facultyCollection.Find(f => f.Id == project.FacultyId).FirstOrDefaultAsync();
                var student = await _studentCollection.Find(s => s.Id == project.StudentId).FirstOrDefaultAsync();
                var projetWithFacultyAndSrudent = new Project
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    FacultyId = project.FacultyId,
                    Faculties= faculty,
                    StudentId = project.StudentId,
                    Student = student,
                };

                projetsWithFacultyAndStudent.Add(projetWithFacultyAndSrudent);
            }

            return Ok(new { data = new { success = true, projects = projetsWithFacultyAndStudent } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProjectId(string id)
        {
            var project = await _projectCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null)
            {
                return NotFound(new { data = new { success = false, message = "This project not found" } });
            }
            var faculty = await _facultyCollection.Find(f => f.Id == project.FacultyId).FirstOrDefaultAsync();
            var student = await _studentCollection.Find(s => s.Id == project.StudentId).FirstOrDefaultAsync();
            var projetWithFacultyAndSrudent = new Project
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                FacultyId = project.FacultyId,
                Faculties = faculty,
                StudentId = project.StudentId,
                Student = student,
            };

            return Ok(new { data = new { success = true, projects = projetWithFacultyAndSrudent } });
        }

        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(Project newProject)
        {
            var faculty = await _facultyCollection.Find(f => f.Id == newProject.FacultyId).FirstOrDefaultAsync();
            if (faculty == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid faculty"}});
            }
            var student = await _studentCollection.Find(s => s.Id == newProject.StudentId).FirstOrDefaultAsync();
            if(student == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid student" } });
            }

            var projectExists = await _projectCollection.Find(a => a.Title == newProject.Title).FirstOrDefaultAsync();
            if (projectExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Project already exists" } });
            }

            newProject.Faculties = faculty;
            newProject.Student = student;
            newProject.IsActive = true;
            newProject.CreatedAt = DateTime.UtcNow;
            await _projectCollection.InsertOneAsync(newProject);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Project created successfully...",
                    project = new
                    {
                        id = newProject.Id,
                        title = newProject.Title,
                        description = newProject.Description,
                        faculty = new
                        {
                            id = faculty.Id,
                            firstName = faculty.FirstName,
                            lastName = faculty.LastName,
                            email = faculty.Email,
                        },
                        student = new
                        {
                            id = student.Id,
                            firstName = student.FirstName,
                            lastName = student.LastName,
                            email = student.Email,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(string id, Project updateProject)
        {
            var existingProject = await _projectCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Project>.Update
                .Set(a => a.Title, updateProject.Title)
                .Set(a => a.Description, updateProject.Description)
                .Set(a => a.FacultyId, updateProject.FacultyId)
                .Set(a => a.StudentId, updateProject.StudentId));
            if (existingProject == null)
            {
                return NotFound(new { data = new { success = false, message = "This project not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Project updated successfully...",
                    project = updateProject
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            var result = await _projectCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This project is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Project deleted successfully..." } });
        }
    }
}

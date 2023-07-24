using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IMongoCollection<Faculty> _facultyCollection;
        private readonly IMongoCollection<Announcement> _announcementCollection;

        public AnnouncementsController(IMongoDatabase database)
        {
            _facultyCollection = database.GetCollection<Faculty>("faculties");
            _announcementCollection = database.GetCollection<Announcement>("announcements");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetAllAnouncements()
        {
            var announcements = await _announcementCollection.Find(a => true).ToListAsync();
            if(announcements.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Announcement not found" } });
            }

            var announcementsWithFaculty = new List<Announcement>();
            foreach(var announcement in announcements)
            {
                var faculty = await _facultyCollection.Find(f => f.Id == announcement.FacultyId).FirstOrDefaultAsync();
                var announcementWithFaculty = new Announcement
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Description = announcement.Description,
                    FacultyId = announcement.FacultyId,
                    Faculties = faculty,
                    IsActive = announcement.IsActive
                };

                announcementsWithFaculty.Add(announcementWithFaculty); 
            }
            return Ok(new { data = new { success = true, announcements = announcementsWithFaculty } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Announcement>> GetAnnouncementById(string id)
        {
            var announcement = await _announcementCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (announcement == null)
            {
                return NotFound(new { data = new { success = false, message = "This announcement was not found"} });
            }

            var faculty = await _facultyCollection.Find(a => a.Id == announcement.FacultyId).FirstOrDefaultAsync();

            var announcementWithFaculty = new Announcement
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Description = announcement.Description,
                FacultyId = announcement.FacultyId,
                Faculties = faculty,
                IsActive = announcement.IsActive
            };

            return Ok(new { data = new {success = true, announcement = announcementWithFaculty} });
        }

        [HttpPost]
        public async Task<ActionResult<Announcement>> CreateAnnouncement(Announcement newAnnouncement)
        {
            var faculty = await _facultyCollection.Find(f => f.Id == newAnnouncement.FacultyId).FirstOrDefaultAsync();
            if(faculty == null)
            {
                return NotFound(new {data = new { success = false, message = "Invalid announcement" }});
            }

            var announcementExists = await _announcementCollection.Find(a => a.Title == newAnnouncement.Title).FirstOrDefaultAsync();
            if (announcementExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Announcements already exists" } });
            }

            newAnnouncement.Faculties = faculty;
            newAnnouncement.IsActive = true;
            newAnnouncement.CreatedAt = DateTime.UtcNow;
            await _announcementCollection.InsertOneAsync(newAnnouncement);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Announcement created successfully",
                    announcement = new
                    {
                        id = newAnnouncement.Id,
                        title = newAnnouncement.Title,
                        description = newAnnouncement.Description,
                        facultyId = newAnnouncement.FacultyId,
                        faculty = new
                        {
                            id = faculty.Id,
                            firstName = faculty.FirstName,
                            lastName = faculty.LastName,
                        },
                        isActive = newAnnouncement.IsActive
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(string id, Announcement updateAnnouncement)
        {
            var existingAnnouncement = await _announcementCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Announcement>.Update
                .Set(a => a.Title, updateAnnouncement.Title)
                .Set(a => a.Description, updateAnnouncement.Description)
                .Set(a => a.FacultyId, updateAnnouncement.FacultyId));
            if (existingAnnouncement == null)
            {
                return NotFound(new { data = new { success = false, message = "This announcement not found" } });
            }

            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Announcement successfully updated...",
                    announcement = updateAnnouncement
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(string id)
        {
            var result = await _announcementCollection.DeleteOneAsync(a => a.Id == id);
            if(result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This announcement not found" } });
            }

            return Ok(new { data = new { success = true, message = "Announcement deleted" } });
        }
    }
}

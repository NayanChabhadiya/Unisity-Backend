using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IMongoCollection<Event> _eventCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;

        public EventsController(IMongoDatabase database)
        {
            _eventCollection = database.GetCollection<Event>("events");
            _organizationCollection = database.GetCollection<Organization>("organizations");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetAllEvents()
        {
            var events = await _eventCollection.Find(e => true).ToListAsync();
            if (events.Count == 0)
            {
                return NotFound(new { messages = "Events not found" });
            }
            var eventsWithOrganization = new List<Event>();

            foreach(var eventa in events)
            {
                var organization = await _organizationCollection.Find(o => o.Id == eventa.OrganizationId).FirstOrDefaultAsync();
                var eventWithOrganization = new Event
                {
                    Id = eventa.Id,
                    Title = eventa.Title,
                    Description = eventa.Description,
                    OrganizationId = eventa.OrganizationId,
                    Organizations = organization
                };
                eventsWithOrganization.Add(eventWithOrganization);
            }

            return Ok(new {data = new { success = true, events = eventsWithOrganization } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(string id)
        {
            var singleEvent = await _eventCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if(singleEvent == null)
            {
                return NotFound(new { data = new {success = false, Message = "This event not found" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id==singleEvent.OrganizationId).FirstOrDefaultAsync();
            if(organization == null)
            {
                return NotFound(new {data = new {success = false, message = "Invalid organization"}});
            }

            var eventWithOrganization = new Event
            {
                Id = singleEvent.Id,
                Title = singleEvent.Title,
                Description = singleEvent.Description,
                OrganizationId = singleEvent.OrganizationId,
                Organizations = organization
            };

            return Ok(new {data = eventWithOrganization});
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event newEvent)
        {
            var organization = await _organizationCollection.Find(o => o.Id == newEvent.OrganizationId).FirstOrDefaultAsync();
            if (organization == null)
            {
                return NotFound(new { data = new {success = false, message = "Invalid organization" } });
            }

            var eventExists = await _eventCollection.Find(a => a.Title == newEvent.Title && a.OrganizationId == organization.Id).FirstOrDefaultAsync();
            if (eventExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "This organization have already exists this event" } });
            }

            newEvent.Organizations = organization;
            newEvent.IsActive = true;
            newEvent.CreatedAt = DateTime.UtcNow;
            await _eventCollection.InsertOneAsync(newEvent);
            return Ok(new {data = new {success = true, message = "Event created successfully", events = newEvent} });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(string id, Event updateEvent)
        {
            var existingEvent = await _eventCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Event>.Update
                .Set(a => a.Title, updateEvent.Title)
                .Set(a => a.Description, updateEvent.Description)
                .Set(a => a.OrganizationId, updateEvent.OrganizationId));
            if (existingEvent == null)
            {
                return NotFound(new { data = new { success = false, Message = "This event not found" } });
            }
            return Ok(new { data = new { success = true, message = "Event updated successfully", events = updateEvent } });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var result = await _eventCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This event is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Event deleted successfully..." } });
        }
    }
}

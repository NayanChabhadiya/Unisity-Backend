using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IMongoCollection<Subscription> _subscriptionCollection;

        public SubscriptionsController(IMongoDatabase database)
        {
            _subscriptionCollection = database.GetCollection<Subscription>("subscriptions");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetAllSubscriptions()
        {
            var subscriptions = await _subscriptionCollection.Find(s => true).ToListAsync();
            if(subscriptions.Count == 0)
            {
                return NotFound(new { data = new { success = false,message = "Subscription not found" } });
            }
            return Ok(subscriptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Subscription>> GetSubscriptionById(string id)
        {
            var subscription = await _subscriptionCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (subscription == null)
            {
                return NotFound(new { data = new { success = false, message = "Subscription not found" } });
            }
            return Ok(subscription);
        }

        [HttpPost]
        public async Task<ActionResult<Subscription>> CreateSubscription(Subscription newSubscription)
        {
            var subscriptionExists = await _subscriptionCollection.Find(a => a.Name == newSubscription.Name).FirstOrDefaultAsync();
            if (subscriptionExists != null)
            {
                return BadRequest(new { data = new { success = false, message = "Subscription already exists" } });
            }

            newSubscription.IsActive = true;
            newSubscription.CreatedAt = DateTime.UtcNow;
            await _subscriptionCollection.InsertOneAsync(newSubscription);
            return Ok(new { data = new { success = true, message = "Subscription successfully created....", subscrition = newSubscription } });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(string id, Subscription updateSubscription)
        {
            var existingSubscription = await _subscriptionCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Subscription>.Update
                .Set(a => a.Name, updateSubscription.Name));
            if (existingSubscription == null)
            {
                return NotFound(new { data = new { success = false, message = "Subscription not found" } });
            }

            return Ok(new { data = new { success = true, message = "Subscription successfully updated....", subscrition = updateSubscription } });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(string id)
        {
            var result = await _subscriptionCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This subscription is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Subscription deleted successfully..." } });
        }
    }
}

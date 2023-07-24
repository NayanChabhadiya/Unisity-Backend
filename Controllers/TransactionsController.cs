using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Transactions;
using Unisity.Models;

namespace Unisity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IMongoCollection<Transactions> _transactionCollection;
        private readonly IMongoCollection<Subscription> _subscriptionCollection;
        private readonly IMongoCollection<Organization> _organizationCollection;

        public TransactionsController(IMongoDatabase database)
        {
            _transactionCollection = database.GetCollection<Transactions>("transactions");
            _subscriptionCollection = database.GetCollection<Subscription>("subscriptions");
            _organizationCollection = database.GetCollection<Organization>("organizations");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transactions>>> GetAllTransaction()
        {
            var transactions = await _transactionCollection.Find(t => true).ToListAsync();
            if (transactions.Count == 0)
            {
                return NotFound(new { data = new { success = false, message = "Transactions not found" } });
            }

            var transactionsWithOrganizationAndSubscription = new List<Transactions>();
            foreach (var transaction in transactions)
            {
                var organization = await _organizationCollection.Find(o => o.Id == transaction.OrganizationId).FirstOrDefaultAsync();
                var subscription = await _subscriptionCollection.Find(s => s.Id == transaction.SubscriptionId).FirstOrDefaultAsync();
                var transactionWithOrganizationAndSubscription = new Transactions
                {
                    Id = transaction.OrganizationId,
                    OrganizationId = transaction.OrganizationId,
                    Organizations = organization,
                    SubscriptionId = transaction.SubscriptionId,
                    Subscriptions = subscription
                };

                transactionsWithOrganizationAndSubscription.Add(transactionWithOrganizationAndSubscription);
            }

            return Ok(new { data = new { success = true, transactions = transactionsWithOrganizationAndSubscription } });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transactions>> GetTransactionById(string id)
        {
            var transaction = await _transactionCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if(transaction == null)
            {
                return NotFound(new { data = new { success = false, message = "This Transaction was not found" } });
            }

            var organization = await _organizationCollection.Find(o => o.Id == transaction.OrganizationId).FirstOrDefaultAsync();
            var subscription = await _subscriptionCollection.Find(s => s.Id == transaction.SubscriptionId).FirstOrDefaultAsync();
            var transactionWithOrganizationAndSubscription = new Transactions
            {
                Id = transaction.OrganizationId,
                OrganizationId = transaction.OrganizationId,
                Organizations = organization,
                SubscriptionId = transaction.SubscriptionId,
                Subscriptions = subscription
            };

            return Ok(new { data = new { success = true, transactions = transactionWithOrganizationAndSubscription } });
        }

        [HttpPost]
        public async Task<ActionResult<Transactions>> CreateTransaction(Transactions newTransaction)
        {
            var organization = await _organizationCollection.Find(o => o.Id == newTransaction.OrganizationId).FirstOrDefaultAsync();
            if(organization == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid organization" } });
            }
            var subscription = await _subscriptionCollection.Find(s => s.Id == newTransaction.SubscriptionId).FirstOrDefaultAsync();
            if (subscription == null)
            {
                return NotFound(new { data = new { success = false, message = "Invalid subscription" } });
            }

            newTransaction.Organizations = organization;
            newTransaction.Subscriptions = subscription;
            newTransaction.CreatedAt = DateTime.UtcNow;
            await _transactionCollection.InsertOneAsync(newTransaction);
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Transaction created successfully...",
                    transaction = new
                    {
                        id = newTransaction.Id,
                        organization = new
                        {
                            id = organization.Id,
                            name = organization.Name,
                            email = organization.Email,
                        },
                        subscription = new
                        {
                            id = subscription.Id,
                            name = subscription.Name,
                        }
                    }
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransactions(string id, Transactions updateTransaction)
        {
            var existingTransaction = await _transactionCollection.FindOneAndUpdateAsync(
                a => a.Id == id,
                Builders<Transactions>.Update
                .Set(a => a.OrganizationId, updateTransaction.OrganizationId)
                .Set(a => a.SubscriptionId, updateTransaction.SubscriptionId));
            if (existingTransaction == null)
            {
                return NotFound(new { data = new { success = false, message = "This Transaction was not found" } });
            }
            return Ok(new
            {
                data = new
                {
                    success = true,
                    message = "Transaction updated successfully...",
                    transaction = updateTransaction
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(string id)
        {
            var result = await _transactionCollection.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { data = new { success = false, message = "This transaction is not found" } });
            }

            return Ok(new { data = new { success = true, message = "Transaction deleted successfully..." } });
        }
    }
}

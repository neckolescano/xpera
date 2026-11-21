using Microsoft.AspNetCore.Mvc;
using Xpera.domain;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-03 Manage Customer Records.
    [Route("api/customers")]
    public class CustomersController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Customers());

        // What the checkout shows: spent, last visit, balance.
        [HttpGet("{id}/card")]
        public IActionResult Card(int id) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == id), "No customer has that ID.");
            var card = StoreDb.CustomerCard(id);
            return new { card.Name, card.UsuallyBuys, card.Spent, card.LastVisit, card.Balance };
        });

        [HttpPost]
        public IActionResult Add(Customer customer) => InStore(() =>
        {
            // The same rules as the app's Add customer form (Xpera.domain/Rules.cs).
            string? problem = Rules.CheckPersonName(customer.FullName) ?? Rules.CheckMobile(customer.Contact);
            Require(problem == null, problem);
            string contact = Rules.FormatMobile(customer.Contact);
            Require(!StoreDb.ContactTaken(contact), "A customer already has this mobile number.");
            StoreDb.AddCustomer(customer.FullName.Trim(), contact, (customer.UsuallyBuys ?? "").Trim());
            return null;
        });
    }
}

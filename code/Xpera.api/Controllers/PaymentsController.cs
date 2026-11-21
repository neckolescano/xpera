using Microsoft.AspNetCore.Mvc;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-05 Record Payment.
    [Route("api/payments")]
    public class PaymentsController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Payments());

        [HttpPost]
        public IActionResult Record(Payment payment) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == payment.CustomerId), "No customer has that ID.");
            Require(payment.Amount > 0, "The amount must be more than zero.");
            StoreDb.RecordPayment(payment.CustomerId, payment.Amount);
            return null;
        }, "Admin", "Cashier");
    }
}

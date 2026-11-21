using Microsoft.AspNetCore.Mvc;
using Xpera.domain;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-06 Manage Inquiries. An inquiry about a product that is not available waits for it.
    [Route("api/inquiries")]
    public class InquiriesController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Inquiries());

        [HttpPost]
        public IActionResult Add(Inquiry inquiry) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == inquiry.CustomerId), "No customer has that ID.");
            Require(inquiry.Question.Trim() != "", "The question is required.");
            StoreDb.AddInquiry(inquiry.CustomerId, inquiry.Question.Trim(), inquiry.ProductId);
            return null;
        });

        [HttpPut("{id}/status")]
        public IActionResult SetStatus(int id, StatusChange change) => InStore(() =>
        {
            Require(Choices.InquiryStatus.Contains(change.Status), "Status must be one of: " + string.Join(", ", Choices.InquiryStatus) + ".");
            StoreDb.SetInquiryStatus(id, change.Status);
            return null;
        });
    }
}

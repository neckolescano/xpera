using Microsoft.AspNetCore.Mvc;
using Xpera.domain;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-07 Manage Complaints (including warranty claims).
    [Route("api/complaints")]
    public class ComplaintsController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Complaints());

        [HttpPost]
        public IActionResult Add(Complaint complaint) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == complaint.CustomerId), "No customer has that ID.");
            Require(complaint.Details.Trim() != "", "The details are required.");
            Require(Choices.ComplaintTypes.Contains(complaint.Type), "Type must be Complaint or Warranty claim.");
            StoreDb.AddComplaint(complaint.CustomerId, complaint.Details.Trim(), complaint.Type);
            return null;
        });

        [HttpPut("{id}/status")]
        public IActionResult SetStatus(int id, StatusChange change) => InStore(() =>
        {
            Require(Choices.ComplaintStatus.Contains(change.Status), "Status must be one of: " + string.Join(", ", Choices.ComplaintStatus) + ".");
            StoreDb.SetComplaintStatus(id, change.Status);
            return null;
        });
    }
}

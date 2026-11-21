using Microsoft.AspNetCore.Mvc;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-08 Record Feedback.
    [Route("api/feedback")]
    public class FeedbackController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Feedback());

        [HttpPost]
        public IActionResult Add(Feedback feedback) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == feedback.CustomerId), "No customer has that ID.");
            Require(feedback.Rating >= 1 && feedback.Rating <= 5, "The rating must be 1 to 5 stars.");
            StoreDb.AddFeedback(feedback.CustomerId, feedback.Rating, feedback.Comment.Trim());
            return null;
        });
    }
}

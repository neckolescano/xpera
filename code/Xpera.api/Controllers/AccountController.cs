using Microsoft.AspNetCore.Mvc;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // GET api/me: checks the headers and says who is signed in, like the desktop login screen.
    [Route("api/me")]
    public class AccountController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult Me() => InStore(() => new
        {
            store = Session.Store.Name,
            branch = Session.Store.Branch,
            plan = "Tenant " + Session.Store.Plan + " · " + Session.Store.PlanName,
            user = Session.UserName,
            role = Session.Role
        });
    }
}

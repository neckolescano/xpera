using Microsoft.AspNetCore.Mvc;
using Xpera.domain.entities;
using SignInResult = Xpera.domain.entities.SignInResult;   // ASP.NET has its own SignInResult too
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // The parent of every store controller. Each request says who is asking, in two headers:
    //   X-Username: cashier01     X-Password: ...
    // Like the desktop login: the Master's CompanyUsers says which store the username belongs to,
    // then the user signs in to that store's database (3 wrong passwords lock the account).
    [ApiController]
    public abstract class StoreControllerBase : ControllerBase
    {
        protected IActionResult InStore(Func<object?> work, params string[] roles)
        {
            string username = Request.Headers["X-Username"].ToString();
            string password = Request.Headers["X-Password"].ToString();
            if (username == "" || password == "")
                return Unauthorized(new { message = "Send the X-Username and X-Password headers." });

            StoreInfo? store = MasterDb.FindStoreForUser(username);
            if (store == null || store.IsMaster) return Unauthorized(new { message = "Wrong username or password." });
            if (store.Status != "Active") return StatusCode(403, new { message = "This store is suspended." });

            try
            {
                Session.Store = store;
                SignInResult result = StoreDb.SignIn(username, password);
                if (result == SignInResult.Locked) return StatusCode(403, new { message = "This account is locked. Ask the store owner to unlock it." });
                if (result == SignInResult.WrongPassword) return Unauthorized(new { message = "Wrong username or password." });
                if (roles.Length > 0 && !roles.Contains(Session.Role))
                    return StatusCode(403, new { message = "Only " + string.Join(" or ", roles) + " can do this." });

                object? answer = work();
                return answer == null ? NoContent() : Ok(answer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            finally
            {
                Session.Clear();   // the next request on this thread starts signed out
            }
        }

        // Stops a request with a 400 Bad Request and a plain message.
        protected static void Require(bool ok, string? message)
        {
            if (!ok) throw new ArgumentException(message);
        }
    }

    // The body for changing a status, like { "status": "Resolved" }.
    public record StatusChange(string Status);
}

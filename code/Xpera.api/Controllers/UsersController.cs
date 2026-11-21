using Microsoft.AspNetCore.Mvc;
using Xpera.domain;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-01 Manage Staff Accounts (Admin only).
    [Route("api/users")]
    public class UsersController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Users(), "Admin");

        [HttpPost]
        public IActionResult Add(NewUser user) => InStore(() =>
        {
            string? problem = Rules.CheckPersonName(user.FullName) ?? Rules.CheckUsername(user.Username?.Trim().ToLower());
            Require(problem == null, problem);
            Require(!StoreDb.UsernameTaken(user.Username), "That username is already taken.");
            Require(Choices.Roles.Contains(user.Role), "Role must be " + string.Join(", ", Choices.Roles) + ".");
            Require(Rules.CheckPassword(user.Password) == null, Rules.CheckPassword(user.Password));
            StoreDb.AddUser(user.FullName.Trim(), user.Username, user.Role, user.Password);
            return null;
        }, "Admin");
    }

    public record NewUser(string FullName, string Username, string Role, string Password);
}

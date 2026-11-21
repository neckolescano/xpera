using Microsoft.AspNetCore.Mvc;
using Xpera.domain;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-02 Manage Products. Admin adds products; Admin or Staff change availability.
    [Route("api/products")]
    public class ProductsController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Products());

        [HttpPost]
        public IActionResult Add(Product product) => InStore(() =>
        {
            Require(product.Name.Trim() != "", "The product name is required.");
            Require(Choices.Categories.Contains(product.Category), "Category must be one of: " + string.Join(", ", Choices.Categories) + ".");
            Require(Rules.CheckText("The product name", product.Name, Rules.NameLength) == null, Rules.CheckText("The product name", product.Name, Rules.NameLength));
            Require(Rules.CheckMoney("The price", product.Price) == null, Rules.CheckMoney("The price", product.Price));
            StoreDb.AddProduct(product.Name.Trim(), product.Category, product.Price);
            return null;
        }, "Admin");

        // Body: { "isAvailable": true }. Answers how many customers were waiting for it.
        [HttpPut("{id}/availability")]
        public IActionResult SetAvailability(int id, AvailabilityChange change) =>
            InStore(() => new { waitingCustomers = StoreDb.SetAvailability(id, change.IsAvailable) }, "Admin", "Staff");
    }

    public record AvailabilityChange(bool IsAvailable);
}

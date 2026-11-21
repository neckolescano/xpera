using Microsoft.AspNetCore.Mvc;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.api.Controllers
{
    // UC-04 Record Sale.
    [Route("api/sales")]
    public class SalesController : StoreControllerBase
    {
        [HttpGet]
        public IActionResult List() => InStore(() => StoreRecords.Sales());

        // Body: { "customerId": 1, "paymentType": "Cash", "items": [ { "productId": 3, "quantity": 1 } ] }
        // Prices always come from the Products table, never from the request.
        [HttpPost]
        public IActionResult Save(Sale sale) => InStore(() =>
        {
            Require(StoreDb.CustomerChoices().Any(c => c.Id == sale.CustomerId), "No customer has that ID.");
            Require(sale.Items.Count > 0, "Add at least one item.");
            Require(sale.PaymentType == "Cash" || sale.PaymentType == "Credit", "Payment type must be Cash or Credit.");
            bool credit = sale.PaymentType == "Credit";
            Require(!credit || sale.DueDate >= DateTime.Today, "A credit sale needs a due date from today on.");

            var products = StoreDb.ProductChoices().ToDictionary(p => p.Id);
            var items = new List<(int ProductId, int Quantity, decimal Price)>();
            foreach (SaleItem item in sale.Items)
            {
                Require(products.TryGetValue(item.ProductId, out var product), "No product has ID " + item.ProductId + ".");
                Require(product.Available, product.Name + " is not available.");
                Require(item.Quantity > 0, "Quantity must be at least 1.");
                items.Add((item.ProductId, item.Quantity, product.Price));
            }
            string receipt = StoreDb.SaveSale(sale.CustomerId, items, credit, sale.DueDate);
            return new { receiptNo = receipt, total = items.Sum(i => i.Quantity * i.Price) };
        }, "Admin", "Cashier");
    }
}

namespace Xpera.domain.entities
{
    // UC-04: one checkout (table Sales). Its products are in Items.
    public class Sale : ISyncTracked
    {
        public int SaleId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;  // like OR-000483
        public DateTime SaleDate { get; set; } = DateTime.Today;
        public int CustomerId { get; set; }
        public int? CashierId { get; set; }
        public string PaymentType { get; set; } = "Cash";      // Cash or Credit
        public decimal Total { get; set; }
        public DateTime? DueDate { get; set; }                 // only for credit sales
        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

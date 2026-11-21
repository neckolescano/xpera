namespace Xpera.domain.entities
{
    // UC-05: money a customer pays toward what they owe (table Payments).
    public class Payment : ISyncTracked
    {
        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidOn { get; set; } = DateTime.Today;
        public int? CashierId { get; set; }
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

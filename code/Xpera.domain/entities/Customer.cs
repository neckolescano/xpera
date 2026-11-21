namespace Xpera.domain.entities
{
    // UC-03: a customer the store remembers (table Customers).
    public class Customer : ISyncTracked
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string UsuallyBuys { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Today;
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

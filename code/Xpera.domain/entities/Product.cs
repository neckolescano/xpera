namespace Xpera.domain.entities
{
    // UC-02: something the store sells (table Products). No stock counts, only available or not.
    public class Product : ISyncTracked
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

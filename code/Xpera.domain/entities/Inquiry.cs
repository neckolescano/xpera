namespace Xpera.domain.entities
{
    // UC-06: a customer's question (table Inquiries). It waits if the product is not available.
    public class Inquiry : ISyncTracked
    {
        public int InquiryId { get; set; }
        public DateTime InquiryDate { get; set; } = DateTime.Today;
        public int CustomerId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public string Status { get; set; } = "Open";           // see Choices.InquiryStatus
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

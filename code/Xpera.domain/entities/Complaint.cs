namespace Xpera.domain.entities
{
    // UC-07: a complaint or warranty claim (table Complaints).
    public class Complaint : ISyncTracked
    {
        public int ComplaintId { get; set; }
        public DateTime ComplaintDate { get; set; } = DateTime.Today;
        public int CustomerId { get; set; }
        public string Details { get; set; } = string.Empty;
        public string Type { get; set; } = "Complaint";        // Complaint or Warranty claim
        public string Status { get; set; } = "Open";           // see Choices.ComplaintStatus
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

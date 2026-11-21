namespace Xpera.domain.entities
{
    // UC-08: a rating from 1 to 5 stars and a comment (table Feedback).
    public class Feedback : ISyncTracked
    {
        public int FeedbackId { get; set; }
        public DateTime FeedbackDate { get; set; } = DateTime.Today;
        public int CustomerId { get; set; }
        public int Rating { get; set; } = 5;
        public string Comment { get; set; } = string.Empty;
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

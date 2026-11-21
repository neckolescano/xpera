namespace Xpera.domain.entities
{
    // One line in the Activity log (table ActivityLogs): what someone did in the store, and when.
    // A line about a record points at it with TableName + RecordId (like "Sales", 412). The page reads that
    // record's IsSynced to show "Synced to cloud" or "Not yet synced", so the sync status is stored only once,
    // on the record itself. Lines like "Signed in" or "Backed up to the cloud" point at no record.
    public class ActivityLog
    {
        public int ActivityLogId { get; set; }
        public DateTime LoggedAt { get; set; } = DateTime.Now;
        public int? UserId { get; set; }                       // who did it
        public string Action { get; set; } = string.Empty;     // like "Recorded sale OR-000480"
        public string? TableName { get; set; }                 // like "Sales", or null
        public int? RecordId { get; set; }                     // the record's ID in that table, or null
    }
}

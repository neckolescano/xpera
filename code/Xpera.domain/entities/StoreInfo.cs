namespace Xpera.domain.entities
{
    // A store found by its store code in the Master database (Companies + CompanyDatabases + Subscriptions).
    // The app keeps the signed-in store in Session.Store.
    public class StoreInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;       // A, B, or C
        public string PlanName { get; set; } = string.Empty;   // Micro, Small, Medium
        public string Database { get; set; } = string.Empty;   // like Xpera_TenantC
        public string Status { get; set; } = string.Empty;     // Active or Suspended
        public bool IsMaster { get; set; }
    }
}

using Xpera.domain.entities;

namespace Xpera.infrastructure.data
{
    // One store's tables as models from Xpera.domain, read through TenantCrmDbContext (EF Core).
    // The API sends these out as JSON. AsNoTracking = read only, EF doesn't watch them for changes.
    public static class StoreRecords
    {
        private static TenantCrmDbContext Open() { return DbContexts.Tenant(Session.Store.Database); }

        public static List<User> Users()
        {
            using TenantCrmDbContext db = Open();
            return db.Users.AsNoTracking().OrderBy(x => x.FullName).ToList();
        }

        public static List<Product> Products()
        {
            using TenantCrmDbContext db = Open();
            return db.Products.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();
        }

        public static List<Customer> Customers()
        {
            using TenantCrmDbContext db = Open();
            return db.Customers.AsNoTracking().OrderBy(x => x.FullName).ToList();
        }

        // Every sale with its items, newest first.
        public static List<Sale> Sales()
        {
            using TenantCrmDbContext db = Open();
            return db.Sales.AsNoTracking().Include(x => x.Items)
                .OrderByDescending(x => x.SaleDate).ThenByDescending(x => x.SaleId).ToList();
        }

        public static List<Payment> Payments()
        {
            using TenantCrmDbContext db = Open();
            return db.Payments.AsNoTracking().OrderByDescending(x => x.PaidOn).ThenByDescending(x => x.PaymentId).ToList();
        }

        public static List<Inquiry> Inquiries()
        {
            using TenantCrmDbContext db = Open();
            return db.Inquiries.AsNoTracking().OrderByDescending(x => x.InquiryDate).ThenByDescending(x => x.InquiryId).ToList();
        }

        public static List<Complaint> Complaints()
        {
            using TenantCrmDbContext db = Open();
            return db.Complaints.AsNoTracking().OrderByDescending(x => x.ComplaintDate).ThenByDescending(x => x.ComplaintId).ToList();
        }

        public static List<Feedback> Feedback()
        {
            using TenantCrmDbContext db = Open();
            return db.Feedback.AsNoTracking().OrderByDescending(x => x.FeedbackDate).ThenByDescending(x => x.FeedbackId).ToList();
        }
    }
}

using Xpera.domain;
using Xpera.domain.entities;

namespace Xpera.infrastructure.data
{
    // The U and D of CRUD: editing and deleting records (StoreDb.cs has the C and R).
    //
    // Every edit and delete:
    //   - goes through TenantCrmDbContext.SaveChanges, so an edited record becomes "Not yet synced";
    //   - writes a line to the Activity log;
    //   - reaches the cloud at the next backup (the backup copies whole tables, so deleted rows disappear there too).
    //
    // A delete that would break other records is refused with a plain reason, instead of a database error.
    // For example a customer with sales can't be deleted: the sales would lose who bought them.
    // Sales and payments are never deleted on purpose: money records stay, like in a real store.
    public static partial class StoreDb
    {
        // ---------- UC-03 customers ----------

        public static Customer GetCustomer(int customerId)
        {
            using TenantCrmDbContext db = Open();
            return db.Customers.Find(customerId);
        }

        public static void UpdateCustomer(int customerId, string fullName, string contact, string usuallyBuys)
        {
            using TenantCrmDbContext db = Open();
            Customer customer = db.Customers.Find(customerId);
            if (customer == null) return;
            customer.FullName = fullName;
            customer.Contact = contact;
            customer.UsuallyBuys = usuallyBuys;
            if (!db.ChangeTracker.HasChanges()) return;     // nothing changed, nothing to log
            db.SaveChanges();
            Log<Customer>(db, "Edited customer " + fullName, customerId);
        }

        // Returns null when deleted, or the reason it can't be. onlyCheck = just say if it could be deleted
        // (the screen asks this first, so it can explain before asking "are you sure?").
        public static string DeleteCustomer(int customerId, bool onlyCheck = false)
        {
            using TenantCrmDbContext db = Open();
            Customer customer = db.Customers.Find(customerId);
            if (customer == null) return null;

            List<string> uses = new List<string>();
            void Count(int n, string one, string many) { if (n > 0) uses.Add(n + " " + (n == 1 ? one : many)); }
            Count(db.Sales.Count(x => x.CustomerId == customerId), "sale", "sales");
            Count(db.Payments.Count(x => x.CustomerId == customerId), "payment", "payments");
            Count(db.Inquiries.Count(x => x.CustomerId == customerId), "inquiry", "inquiries");
            Count(db.Complaints.Count(x => x.CustomerId == customerId), "complaint", "complaints");
            Count(db.Feedback.Count(x => x.CustomerId == customerId), "feedback", "feedback");
            if (uses.Count > 0)
                return customer.FullName + " has " + string.Join(", ", uses) + ". Those records need a customer, so " +
                       customer.FullName + " can't be deleted.";
            if (onlyCheck) return null;

            db.Customers.Remove(customer);
            db.SaveChanges();
            Log<Customer>(db, "Deleted customer " + customer.FullName, customerId);
            return null;
        }

        // ---------- UC-02 products ----------

        public static Product GetProduct(int productId)
        {
            using TenantCrmDbContext db = Open();
            return db.Products.Find(productId);
        }

        public static void UpdateProduct(int productId, string name, string category, decimal price)
        {
            using TenantCrmDbContext db = Open();
            Product product = db.Products.Find(productId);
            if (product == null) return;
            product.Name = name;
            product.Category = category;
            product.Price = price;
            if (!db.ChangeTracker.HasChanges()) return;
            db.SaveChanges();
            Log<Product>(db, "Edited product " + name + " · " + Money.Peso(price), productId);
        }

        public static string DeleteProduct(int productId, bool onlyCheck = false)
        {
            using TenantCrmDbContext db = Open();
            Product product = db.Products.Find(productId);
            if (product == null) return null;

            int sold = db.SaleItems.Count(x => x.ProductId == productId);
            if (sold > 0)
                return product.Name + " is in " + sold + (sold == 1 ? " sale" : " sales") +
                       ", so it can't be deleted. Set it to Not available instead.";
            int asked = db.Inquiries.Count(x => x.ProductId == productId);
            if (asked > 0)
                return asked + (asked == 1 ? " inquiry is" : " inquiries are") + " about " + product.Name + ", so it can't be deleted.";
            if (onlyCheck) return null;

            db.Products.Remove(product);
            db.SaveChanges();
            Log<Product>(db, "Deleted product " + product.Name, productId);
            return null;
        }

        // ---------- UC-01 staff accounts ----------

        public static User GetUser(int userId)
        {
            using TenantCrmDbContext db = Open();
            return db.Users.Find(userId);
        }

        // The last Admin must stay an Admin, or nobody could manage the store.
        public static string UpdateUser(int userId, string fullName, string role)
        {
            using TenantCrmDbContext db = Open();
            User user = db.Users.Find(userId);
            if (user == null) return null;
            if (user.Role == "Admin" && role != "Admin" && db.Users.Count(u => u.Role == "Admin") == 1)
                return user.FullName + " is the only Admin, so the role has to stay Admin.";
            user.FullName = fullName;
            user.Role = role;
            if (!db.ChangeTracker.HasChanges()) return null;
            db.SaveChanges();
            Log<User>(db, "Edited account " + user.Username + " (" + role + ")", userId);
            return null;
        }

        // Locked after 3 wrong passwords, or locked by the Admin. Unlocking also resets the wrong-password count.
        public static string SetUserLocked(int userId, bool locked)
        {
            using TenantCrmDbContext db = Open();
            User user = db.Users.Find(userId);
            if (user == null) return null;
            if (locked && userId == Session.UserId) return "You can't lock your own account.";
            if (locked && user.Role == "Admin" && db.Users.Count(u => u.Role == "Admin" && u.Status == "Active") == 1)
                return user.FullName + " is the only active Admin, so the account can't be locked.";
            user.Status = locked ? "Locked" : "Active";
            user.FailedLogins = 0;
            db.SaveChanges();
            Log<User>(db, (locked ? "Locked" : "Unlocked") + " account " + user.Username, userId);
            return null;
        }

        public static void ResetPassword(int userId, string newPassword)
        {
            using TenantCrmDbContext db = Open();
            User user = db.Users.Find(userId);
            if (user == null) return;
            user.PasswordHash = Db.HashPassword(user.Username, newPassword);
            user.FailedLogins = 0;
            db.SaveChanges();
            Log<User>(db, "Reset the password of " + user.Username, userId);
        }

        // An account that already did something (sales, payments, activity) stays, so the history keeps its name.
        // Lock it instead. A new account that was never used can be deleted; its username leaves the Master too.
        public static string DeleteUser(int userId, bool onlyCheck = false)
        {
            using TenantCrmDbContext db = Open();
            User user = db.Users.Find(userId);
            if (user == null) return null;
            if (userId == Session.UserId) return "You can't delete your own account.";
            if (user.Role == "Admin" && db.Users.Count(u => u.Role == "Admin") == 1)
                return user.FullName + " is the only Admin, so the account can't be deleted.";
            bool used = db.Sales.Any(x => x.CashierId == userId) || db.Payments.Any(x => x.CashierId == userId)
                        || db.ActivityLogs.Any(x => x.UserId == userId);
            if (used)
                return user.FullName + " already has sales or activity in the store, so the account can't be deleted. Lock it instead.";
            if (onlyCheck) return null;

            db.Users.Remove(user);
            db.SaveChanges();
            MasterDb.RemoveUsername(user.Username);      // the username can be used again
            Log<User>(db, "Deleted account " + user.Username, userId);
            return null;
        }

        // ---------- UC-06 inquiries ----------

        public static Inquiry GetInquiry(int inquiryId)
        {
            using TenantCrmDbContext db = Open();
            return db.Inquiries.Find(inquiryId);
        }

        public static void UpdateInquiry(int inquiryId, int customerId, string question, int? productId)
        {
            using TenantCrmDbContext db = Open();
            Inquiry inquiry = db.Inquiries.Find(inquiryId);
            if (inquiry == null) return;
            inquiry.CustomerId = customerId;
            inquiry.Question = question;
            inquiry.ProductId = productId;
            if (!db.ChangeTracker.HasChanges()) return;
            db.SaveChanges();
            Log<Inquiry>(db, "Edited inquiry: " + Short(question), inquiryId);
        }

        public static void DeleteInquiry(int inquiryId)
        {
            using TenantCrmDbContext db = Open();
            Inquiry inquiry = db.Inquiries.Find(inquiryId);
            if (inquiry == null) return;
            db.Inquiries.Remove(inquiry);
            db.SaveChanges();
            Log<Inquiry>(db, "Deleted inquiry: " + Short(inquiry.Question), inquiryId);
        }

        // ---------- UC-07 complaints ----------

        public static Complaint GetComplaint(int complaintId)
        {
            using TenantCrmDbContext db = Open();
            return db.Complaints.Find(complaintId);
        }

        public static void UpdateComplaint(int complaintId, int customerId, string details, string type)
        {
            using TenantCrmDbContext db = Open();
            Complaint complaint = db.Complaints.Find(complaintId);
            if (complaint == null) return;
            complaint.CustomerId = customerId;
            complaint.Details = details;
            complaint.Type = type;
            if (!db.ChangeTracker.HasChanges()) return;
            db.SaveChanges();
            Log<Complaint>(db, "Edited " + type.ToLower() + ": " + Short(details), complaintId);
        }

        public static void DeleteComplaint(int complaintId)
        {
            using TenantCrmDbContext db = Open();
            Complaint complaint = db.Complaints.Find(complaintId);
            if (complaint == null) return;
            db.Complaints.Remove(complaint);
            db.SaveChanges();
            Log<Complaint>(db, "Deleted " + complaint.Type.ToLower() + ": " + Short(complaint.Details), complaintId);
        }

        // ---------- UC-08 feedback ----------

        public static Feedback GetFeedback(int feedbackId)
        {
            using TenantCrmDbContext db = Open();
            return db.Feedback.Find(feedbackId);
        }

        public static void UpdateFeedback(int feedbackId, int customerId, int rating, string comment)
        {
            using TenantCrmDbContext db = Open();
            Feedback feedback = db.Feedback.Find(feedbackId);
            if (feedback == null) return;
            feedback.CustomerId = customerId;
            feedback.Rating = rating;
            feedback.Comment = comment;
            if (!db.ChangeTracker.HasChanges()) return;
            db.SaveChanges();
            Log<Feedback>(db, "Edited feedback (" + rating + " stars)", feedbackId);
        }

        public static void DeleteFeedback(int feedbackId)
        {
            using TenantCrmDbContext db = Open();
            Feedback feedback = db.Feedback.Find(feedbackId);
            if (feedback == null) return;
            db.Feedback.Remove(feedback);
            db.SaveChanges();
            Log<Feedback>(db, "Deleted " + feedback.Rating + "-star feedback", feedbackId);
        }
    }
}

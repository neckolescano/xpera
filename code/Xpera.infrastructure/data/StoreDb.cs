using Xpera.domain;
using Xpera.domain.entities;
using Xpera.infrastructure.services;

namespace Xpera.infrastructure.data
{
    // Who is signed in, and to which store.
    // [ThreadStatic] gives each thread its own copy: the desktop app has one thread, so nothing changes there,
    // while the API serves many people at once and each request sets and clears its own copy.
    public static class Session
    {
        [ThreadStatic] public static StoreInfo Store;
        [ThreadStatic] public static int UserId;
        [ThreadStatic] public static string UserName;
        [ThreadStatic] public static string Role;      // Admin, Cashier, or Staff

        public static void Clear()
        {
            Store = null;
            UserId = 0;
            UserName = null;
            Role = null;
        }
    }

    // Rows of the two views (CustomerSummary, CustomerBalances). A view is a saved SELECT made by the
    // InitialTenantCrm migration, so these are read with SqlQuery instead of being tables in the DbContext.
    public class CustomerSummaryRow
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Contact { get; set; }
        public string UsuallyBuys { get; set; }
        public decimal Spent { get; set; }
        public DateTime? LastVisit { get; set; }
    }

    public class CustomerBalanceRow
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public decimal Balance { get; set; }
        public DateTime? DueDate { get; set; }
    }

    // Everything the screens do inside one store's database, through TenantCrmDbContext (EF Core).
    // Each method opens the signed-in store's database, does one job, and closes it again.
    // Lists come back as (Id, Cells): the Id is the record's key, the Cells are what the table shows.
    public static partial class StoreDb
    {
        // The signed-in store's database, like Xpera_TenantA.
        private static TenantCrmDbContext Open()
        {
            return DbContexts.Tenant(Session.Store.Database);
        }

        // The first words of a long text, so a log line stays one line.
        private static string Short(string text)
        {
            return text.Length <= 50 ? text : text.Substring(0, 47).TrimEnd() + "…";
        }

        private static string Day(DateTime? date)
        {
            return date == null ? "—" : date.Value.ToString("MMM d");
        }

        private static string Peso(decimal amount)
        {
            return Money.Peso(amount);
        }

        // ---------- Activity log ----------

        // Writes one line to the Activity log, about a record of type T with this ID (like Sales, 412).
        // The table name comes from the model, so it always matches the real table.
        private static void Log<T>(TenantCrmDbContext db, string action, int recordId)
        {
            string table = db.Model.FindEntityType(typeof(T)).GetTableName();
            db.ActivityLogs.Add(new ActivityLog { UserId = Who(), Action = action, TableName = table, RecordId = recordId });
            db.SaveChanges();
        }

        private static int? Who()
        {
            return Session.UserId == 0 ? null : Session.UserId;
        }

        // A line about no record: "Signed in", "Signed out", "Backed up to the cloud".
        public static void LogEvent(string action)
        {
            using TenantCrmDbContext db = Open();
            db.ActivityLogs.Add(new ActivityLog { UserId = Who(), Action = action });
            db.SaveChanges();
        }

        // Every log line, newest first: when, who, what, and whether the record it is about is in the cloud yet.
        // The status is read from the record's own IsSynced (it is not stored in the log), so after a backup
        // every line about a backed-up record turns to "Synced to cloud" by itself.
        public static List<(int Index, object[] Cells)> Activity()
        {
            using TenantCrmDbContext db = Open();
            HashSet<(string Table, int Id)> notSynced = NotSyncedRecords(db);
            notSynced.UnionWith(PendingDeletes(db));

            var lines = (from l in db.ActivityLogs
                         join u in db.Users on l.UserId equals (int?)u.UserId into doneBy
                         from u in doneBy.DefaultIfEmpty()          // a LEFT JOIN: a line may have no user
                         orderby l.LoggedAt descending, l.ActivityLogId descending
                         select new { l.ActivityLogId, l.LoggedAt, Who = u == null ? "—" : u.FullName, l.Action, l.TableName, l.RecordId })
                        .ToList();

            return lines.Select(l => (l.ActivityLogId, new object[]
            {
                l.LoggedAt.ToString("MMM d, h:mm tt"), l.Who, l.Action,
                l.TableName == null ? "—" : notSynced.Contains((l.TableName, l.RecordId ?? 0)) ? "Not yet synced" : "Synced to cloud"
            })).ToList();
        }

        // How many records in each table are not in the cloud yet, like ("Sales", 3), most first. Tables with 0 are left out.
        public static List<(string Table, int Count)> NotSyncedCounts()
        {
            using TenantCrmDbContext db = Open();
            HashSet<(string Table, int Id)> waiting = NotSyncedRecords(db);
            waiting.UnionWith(PendingDeletes(db));
            return waiting.GroupBy(r => r.Table)
                .Select(g => (g.Key, g.Count())).OrderByDescending(t => t.Item2).ToList();
        }

        // Records deleted on this PC since the last backup. A deleted row has no IsSynced left to read,
        // but the cloud still has it until the next backup, so its log lines say "Not yet synced" until then.
        private static HashSet<(string Table, int Id)> PendingDeletes(TenantCrmDbContext db)
        {
            DateTime? lastBackup = CloudBackup.LoadStatus(Session.Store?.Code ?? "").LastSuccess;
            var deleted = db.ActivityLogs
                .Where(l => l.TableName != null && l.RecordId != null && l.Action.StartsWith("Deleted")
                            && (lastBackup == null || l.LoggedAt > lastBackup))
                .Select(l => new { l.TableName, l.RecordId })
                .ToList();
            return new HashSet<(string, int)>(deleted.Select(d => (d.TableName, d.RecordId.Value)));
        }

        // Every record with IsSynced = false, as (table, ID). One line per table that has the flag (see ISyncTracked).
        private static HashSet<(string Table, int Id)> NotSyncedRecords(TenantCrmDbContext db)
        {
            HashSet<(string, int)> records = new HashSet<(string, int)>();
            void Add<T>(IQueryable<int> ids)
            {
                string table = db.Model.FindEntityType(typeof(T)).GetTableName();
                foreach (int id in ids) records.Add((table, id));
            }

            Add<User>(db.Users.Where(x => !x.IsSynced).Select(x => x.UserId));
            Add<Product>(db.Products.Where(x => !x.IsSynced).Select(x => x.ProductId));
            Add<Customer>(db.Customers.Where(x => !x.IsSynced).Select(x => x.CustomerId));
            Add<Sale>(db.Sales.Where(x => !x.IsSynced).Select(x => x.SaleId));
            Add<Payment>(db.Payments.Where(x => !x.IsSynced).Select(x => x.PaymentId));
            Add<Inquiry>(db.Inquiries.Where(x => !x.IsSynced).Select(x => x.InquiryId));
            Add<Complaint>(db.Complaints.Where(x => !x.IsSynced).Select(x => x.ComplaintId));
            Add<Feedback>(db.Feedback.Where(x => !x.IsSynced).Select(x => x.FeedbackId));
            return records;
        }

        // ---------- sign-in (an account locks after 3 wrong passwords) ----------

        public static SignInResult SignIn(string username, string password)
        {
            username = username.Trim().ToLower();
            using TenantCrmDbContext db = Open();

            db.KeepSyncFlags = true;   // the failed-login counter is not store data, so it doesn't make the account "not yet synced"
            User user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return SignInResult.WrongPassword;
            if (user.Status == "Locked") return SignInResult.Locked;

            if (!user.PasswordHash.SequenceEqual(Db.HashPassword(username, password)))
            {
                user.FailedLogins++;
                if (user.FailedLogins >= 3) user.Status = "Locked";
                db.SaveChanges();
                return user.Status == "Locked" ? SignInResult.Locked : SignInResult.WrongPassword;
            }

            user.FailedLogins = 0;
            db.SaveChanges();
            Session.UserId = user.UserId;
            Session.UserName = user.FullName;
            Session.Role = user.Role;
            return SignInResult.Ok;
        }

        // ---------- UC-01 staff accounts ----------

        public static List<(int Index, object[] Cells)> Users()
        {
            using TenantCrmDbContext db = Open();
            return db.Users.OrderBy(u => u.FullName).ToList()
                .Select(u => (u.UserId, new object[] { u.FullName, u.Username, u.Role, u.Status })).ToList();
        }

        // Checked in the Add user form, so the problem shows before saving. Usernames are unique in ALL of xpera
        // (people sign in with only a username), so this asks the Master, plus this store in case it is out of step.
        public static bool UsernameTaken(string username)
        {
            username = username.Trim().ToLower();
            if (MasterDb.UsernameTaken(username)) return true;
            using TenantCrmDbContext db = Open();
            return db.Users.Any(u => u.Username == username);
        }

        public static void AddUser(string fullName, string username, string role, string password)
        {
            using TenantCrmDbContext db = Open();
            User user = new User
            {
                FullName = fullName,
                Username = username.Trim().ToLower(),
                PasswordHash = Db.HashPassword(username, password),
                Role = role
            };
            // 1. The Master's CompanyUsers first: its unique index makes sure no other store has this username.
            // 2. Then this store's Users table. If that fails, the Master row is taken back out.
            MasterDb.AddUsername(Session.Store.Code, user.Username);
            try
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
            catch
            {
                MasterDb.RemoveUsername(user.Username);
                throw;
            }
            Log<User>(db, "Added " + role + " account " + user.Username, user.UserId);
        }

        // ---------- UC-02 products ----------

        public static List<(int Index, object[] Cells)> Products()
        {
            using TenantCrmDbContext db = Open();
            var products = db.Products
                .OrderBy(p => p.Category).ThenBy(p => p.Name)
                .Select(p => new
                {
                    p.ProductId, p.Name, p.Category, p.Price, p.IsAvailable,
                    Waiting = db.Inquiries.Count(q => q.ProductId == p.ProductId && q.Status == "Waiting for product")
                })
                .ToList();

            return products.Select(p => (p.ProductId, new object[]
            {
                p.Name, p.Category, Peso(p.Price), p.IsAvailable ? "Available" : "Not available",
                p.Waiting == 0 ? "—" : p.Waiting + " waiting"
            })).ToList();
        }

        // Products for the checkout list: id, name, price, available.
        public static List<(int Id, string Name, decimal Price, bool Available)> ProductChoices()
        {
            using TenantCrmDbContext db = Open();
            return db.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList()
                .Select(p => (p.ProductId, p.Name, p.Price, p.IsAvailable)).ToList();
        }

        public static bool ProductNameTaken(string name, int exceptProductId = 0)
        {
            name = name.Trim();
            using TenantCrmDbContext db = Open();
            return db.Products.Any(p => p.Name == name && p.ProductId != exceptProductId);
        }

        public static void AddProduct(string name, string category, decimal price)
        {
            using TenantCrmDbContext db = Open();
            Product product = new Product { Name = name, Category = category, Price = price };
            db.Products.Add(product);
            db.SaveChanges();
            Log<Product>(db, "Added product " + name, product.ProductId);
        }

        // Returns how many customers were waiting for it, so the screen can remind staff to call them.
        public static int SetAvailability(int productId, bool available)
        {
            using TenantCrmDbContext db = Open();
            Product product = db.Products.Find(productId);
            if (product != null && product.IsAvailable != available)
            {
                product.IsAvailable = available;
                db.SaveChanges();
                Log<Product>(db, product.Name + " set to " + (available ? "Available" : "Not available"), productId);
            }
            return db.Inquiries.Count(q => q.ProductId == productId && q.Status == "Waiting for product");
        }

        // ---------- UC-03 customers ----------

        public static List<(int Index, object[] Cells)> Customers()
        {
            using TenantCrmDbContext db = Open();
            return db.Database.SqlQuery<CustomerSummaryRow>($"SELECT CustomerId, FullName, Contact, UsuallyBuys, Spent, LastVisit FROM CustomerSummary")
                .OrderBy(c => c.FullName).ToList()
                .Select(c => (c.CustomerId, new object[] { c.FullName, c.Contact, c.UsuallyBuys, Peso(c.Spent), Day(c.LastVisit) })).ToList();
        }

        public static List<(int Id, string Name)> CustomerChoices()
        {
            using TenantCrmDbContext db = Open();
            return db.Customers.OrderBy(c => c.FullName).ToList()
                .Select(c => (c.CustomerId, c.FullName)).ToList();
        }

        // What the checkout shows about a customer.
        public static (string Name, string UsuallyBuys, decimal Spent, DateTime? LastVisit, decimal Balance) CustomerCard(int customerId)
        {
            using TenantCrmDbContext db = Open();
            CustomerSummaryRow summary = db.Database.SqlQuery<CustomerSummaryRow>(
                $"SELECT CustomerId, FullName, Contact, UsuallyBuys, Spent, LastVisit FROM CustomerSummary WHERE CustomerId = {customerId}").ToList()[0];
            CustomerBalanceRow balance = db.Database.SqlQuery<CustomerBalanceRow>(
                $"SELECT CustomerId, FullName, Balance, DueDate FROM CustomerBalances WHERE CustomerId = {customerId}").ToList()[0];
            return (summary.FullName, summary.UsuallyBuys, summary.Spent, summary.LastVisit, balance.Balance);
        }

        // One customer per mobile number. Numbers are saved like 0956 563 4228 (Rules.FormatMobile).
        public static bool ContactTaken(string contact, int exceptCustomerId = 0)
        {
            contact = contact.Trim();
            using TenantCrmDbContext db = Open();
            return db.Customers.Any(c => c.Contact == contact && c.CustomerId != exceptCustomerId);
        }

        public static void AddCustomer(string fullName, string contact, string usuallyBuys)
        {
            using TenantCrmDbContext db = Open();
            Customer customer = new Customer { FullName = fullName, Contact = contact, UsuallyBuys = usuallyBuys };
            db.Customers.Add(customer);
            db.SaveChanges();
            Log<Customer>(db, "Added customer " + fullName, customer.CustomerId);
        }

        // ---------- UC-04 sales ----------

        public static List<(int Index, object[] Cells)> Sales()
        {
            using TenantCrmDbContext db = Open();
            var sales = (from s in db.Sales
                         join c in db.Customers on s.CustomerId equals c.CustomerId
                         orderby s.SaleDate descending, s.SaleId descending
                         select new { s.SaleId, s.ReceiptNo, s.SaleDate, c.FullName, s.PaymentType, s.Total })
                        .ToList();

            // The products in every sale, like "2 x Gaming mouse, Wireless headset".
            ILookup<int, string> items = (from i in db.SaleItems
                                          join p in db.Products on i.ProductId equals p.ProductId
                                          orderby i.SaleItemId
                                          select new { i.SaleId, i.Quantity, p.Name })
                                         .ToList()
                                         .ToLookup(i => i.SaleId, i => (i.Quantity > 1 ? i.Quantity + " x " : "") + i.Name);

            return sales.Select(s => (s.SaleId, new object[]
            {
                s.ReceiptNo, Day(s.SaleDate), s.FullName, string.Join(", ", items[s.SaleId]), s.PaymentType, Peso(s.Total)
            })).ToList();
        }

        public static string NextReceiptNo()
        {
            using TenantCrmDbContext db = Open();
            return NextReceiptNo(db);
        }

        // Receipts look like OR-000483. The numbers have the same width, so the highest text is the highest number.
        private static string NextReceiptNo(TenantCrmDbContext db)
        {
            string last = db.Sales.Max(s => (string)s.ReceiptNo);
            int number = last == null ? 0 : int.Parse(last.Substring(3));
            return "OR-" + (number + 1).ToString("000000");
        }

        // Saves the sale and its items together. If anything fails, nothing is saved.
        public static string SaveSale(int customerId, List<(int ProductId, int Quantity, decimal Price)> items, bool credit, DateTime? dueDate)
        {
            using TenantCrmDbContext db = Open();
            using var transaction = db.Database.BeginTransaction();

            string receipt = NextReceiptNo(db);
            Sale sale = new Sale
            {
                ReceiptNo = receipt,
                CustomerId = customerId,
                CashierId = Session.UserId,
                PaymentType = credit ? "Credit" : "Cash",
                Total = items.Sum(i => i.Quantity * i.Price),
                DueDate = credit ? dueDate : null,
                Items = items.Select(i => new SaleItem { ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.Price }).ToList()
            };
            db.Sales.Add(sale);         // EF saves the sale first, then its items with the new SaleId
            db.SaveChanges();
            Log<Sale>(db, "Recorded " + sale.PaymentType.ToLower() + " sale " + receipt + " · " + Peso(sale.Total), sale.SaleId);

            transaction.Commit();
            return receipt;
        }

        // ---------- UC-05 payments ----------

        // Customers who still owe something. Overdue ones come first.
        public static List<(int Index, object[] Cells)> Balances()
        {
            using TenantCrmDbContext db = Open();
            DateTime today = DateTime.Today;
            return db.Database.SqlQuery<CustomerBalanceRow>($"SELECT CustomerId, FullName, Balance, DueDate FROM CustomerBalances")
                .Where(b => b.Balance > 0).OrderBy(b => b.DueDate).ToList()
                .Select(b => (b.CustomerId, new object[]
                {
                    b.FullName, Peso(b.Balance), Day(b.DueDate),
                    b.DueDate < today ? "Overdue" : b.DueDate <= today.AddDays(7) ? "Due soon" : "On track"
                })).ToList();
        }

        public static void RecordPayment(int customerId, decimal amount)
        {
            using TenantCrmDbContext db = Open();
            Payment payment = new Payment { CustomerId = customerId, Amount = amount, CashierId = Session.UserId };
            db.Payments.Add(payment);
            db.SaveChanges();
            string name = db.Customers.Where(c => c.CustomerId == customerId).Select(c => c.FullName).FirstOrDefault();
            Log<Payment>(db, "Recorded payment " + Peso(amount) + " from " + name, payment.PaymentId);
        }

        // ---------- UC-06 inquiries ----------

        public static List<(int Index, object[] Cells)> Inquiries()
        {
            using TenantCrmDbContext db = Open();
            return (from q in db.Inquiries
                    join c in db.Customers on q.CustomerId equals c.CustomerId
                    join p in db.Products on q.ProductId equals (int?)p.ProductId into asked
                    from p in asked.DefaultIfEmpty()          // a LEFT JOIN: not every inquiry is about a product
                    orderby q.InquiryDate descending, q.InquiryId descending
                    select new { q.InquiryId, q.InquiryDate, c.FullName, q.Question, Product = p == null ? "—" : p.Name, q.Status })
                   .ToList()
                   .Select(q => (q.InquiryId, new object[] { Day(q.InquiryDate), q.FullName, q.Question, q.Product, q.Status })).ToList();
        }

        // A product that is not available keeps the inquiry waiting.
        public static void AddInquiry(int customerId, string question, int? productId)
        {
            using TenantCrmDbContext db = Open();
            bool waiting = productId != null && db.Products.Any(p => p.ProductId == productId && !p.IsAvailable);
            Inquiry inquiry = new Inquiry
            {
                CustomerId = customerId,
                Question = question,
                ProductId = productId,
                Status = waiting ? "Waiting for product" : "Open"
            };
            db.Inquiries.Add(inquiry);
            db.SaveChanges();
            Log<Inquiry>(db, "Logged inquiry: " + Short(question), inquiry.InquiryId);
        }

        public static void SetInquiryStatus(int inquiryId, string status)
        {
            using TenantCrmDbContext db = Open();
            Inquiry inquiry = db.Inquiries.Find(inquiryId);
            if (inquiry == null || inquiry.Status == status) return;   // nothing changed, nothing to log
            inquiry.Status = status;
            db.SaveChanges();
            Log<Inquiry>(db, "Inquiry set to " + status + ": " + Short(inquiry.Question), inquiryId);
        }

        // ---------- UC-07 complaints ----------

        public static List<(int Index, object[] Cells)> Complaints()
        {
            using TenantCrmDbContext db = Open();
            return (from k in db.Complaints
                    join c in db.Customers on k.CustomerId equals c.CustomerId
                    orderby k.ComplaintDate descending, k.ComplaintId descending
                    select new { k.ComplaintId, k.ComplaintDate, c.FullName, k.Details, k.Type, k.Status })
                   .ToList()
                   .Select(k => (k.ComplaintId, new object[] { Day(k.ComplaintDate), k.FullName, k.Details, k.Type, k.Status })).ToList();
        }

        public static void AddComplaint(int customerId, string details, string type)
        {
            using TenantCrmDbContext db = Open();
            Complaint complaint = new Complaint { CustomerId = customerId, Details = details, Type = type };
            db.Complaints.Add(complaint);
            db.SaveChanges();
            Log<Complaint>(db, "Logged " + type.ToLower() + ": " + Short(details), complaint.ComplaintId);
        }

        public static void SetComplaintStatus(int complaintId, string status)
        {
            using TenantCrmDbContext db = Open();
            Complaint complaint = db.Complaints.Find(complaintId);
            if (complaint == null || complaint.Status == status) return;
            complaint.Status = status;
            db.SaveChanges();
            Log<Complaint>(db, complaint.Type + " set to " + status + ": " + Short(complaint.Details), complaintId);
        }

        // ---------- UC-08 feedback ----------

        public static List<(int Index, object[] Cells)> Feedback()
        {
            using TenantCrmDbContext db = Open();
            return (from f in db.Feedback
                    join c in db.Customers on f.CustomerId equals c.CustomerId
                    orderby f.FeedbackDate descending, f.FeedbackId descending
                    select new { f.FeedbackId, f.FeedbackDate, c.FullName, f.Rating, f.Comment })
                   .ToList()
                   .Select(f => (f.FeedbackId, new object[]
                   {
                       Day(f.FeedbackDate), f.FullName, new string('★', f.Rating) + new string('☆', 5 - f.Rating), f.Comment
                   })).ToList();
        }

        public static void AddFeedback(int customerId, int rating, string comment)
        {
            using TenantCrmDbContext db = Open();
            Feedback feedback = new Feedback { CustomerId = customerId, Rating = rating, Comment = comment };
            db.Feedback.Add(feedback);
            db.SaveChanges();
            Log<Feedback>(db, "Recorded " + rating + "-star feedback", feedback.FeedbackId);
        }
    }
}

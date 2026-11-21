using Microsoft.AspNetCore.Identity;
using Xpera.domain.entities;

namespace Xpera.infrastructure.data
{
    // The Master database (Xpera_Master), read through MasterCrmDbContext:
    // every company (store), its plan, and which database holds its data.
    public static class MasterDb
    {
        // Finds a store by its code, like PIXEL01 (the cloud backup and the Super Admin use codes; people sign in by username).
        public static StoreInfo FindStore(string code)
        {
            code = code.Trim().ToUpper();
            if (code == "") return null;
            if (code == "MASTER") return new StoreInfo { Code = code, Name = "xpera Master", Branch = "Super Admin", IsMaster = true };

            using MasterCrmDbContext db = DbContexts.Master();
            Company company = db.Companies.SingleOrDefault(c => c.CompanyCode == code);
            return company == null ? null : StoreOf(db, company);
        }

        // Sign-in with only a username and password: the Master's CompanyUsers says which store the
        // username belongs to. The Super Admin is in AspNetUsers instead, so they get the Master.
        // Returns null when nobody has that username (the login screen then just says "wrong username or password").
        public static StoreInfo FindStoreForUser(string username)
        {
            username = (username ?? "").Trim().ToLower();
            if (username == "") return null;
            using MasterCrmDbContext db = DbContexts.Master();

            Company company = db.CompanyUsers
                .Where(u => u.Username == username)
                .Select(u => u.Company)
                .FirstOrDefault();
            if (company != null) return StoreOf(db, company);

            string normalized = username.ToUpperInvariant();
            if (db.Users.Any(u => u.NormalizedUserName == normalized))
                return new StoreInfo { Code = "MASTER", Name = "xpera Master", Branch = "Super Admin", IsMaster = true };
            return null;
        }

        // ---------- keeping CompanyUsers in step with each store's Users table ----------

        // Usernames are unique in all of xpera (every store and the Super Admin), not just in one store.
        public static bool UsernameTaken(string username)
        {
            username = (username ?? "").Trim().ToLower();
            string normalized = username.ToUpperInvariant();
            using MasterCrmDbContext db = DbContexts.Master();
            return db.CompanyUsers.Any(u => u.Username == username) || db.Users.Any(u => u.NormalizedUserName == normalized);
        }

        // Called when a store adds a user. The unique index refuses a username another store already has.
        public static void AddUsername(string storeCode, string username)
        {
            using MasterCrmDbContext db = DbContexts.Master();
            int companyId = db.Companies.Where(c => c.CompanyCode == storeCode).Select(c => c.CompanyId).Single();
            db.CompanyUsers.Add(new CompanyUser { CompanyId = companyId, Username = username.Trim().ToLower() });
            db.SaveChanges();
        }

        // Undoes AddUsername when the store's own database could not save the user.
        public static void RemoveUsername(string username)
        {
            username = username.Trim().ToLower();
            using MasterCrmDbContext db = DbContexts.Master();
            CompanyUser row = db.CompanyUsers.FirstOrDefault(u => u.Username == username);
            if (row == null) return;
            db.CompanyUsers.Remove(row);
            db.SaveChanges();
        }

        // Everything the app needs to know about a store: its database, plan, and status.
        private static StoreInfo StoreOf(MasterCrmDbContext db, Company company)
        {
            CompanyDatabase database = db.CompanyDatabases
                .FirstOrDefault(d => d.CompanyId == company.CompanyId && d.IsActive);

            // The active subscription decides the plan (A, B, or C).
            Subscription subscription = db.Subscriptions
                .Where(s => s.CompanyId == company.CompanyId && s.IsActive)
                .OrderByDescending(s => s.StartedOn)
                .Select(s => new Subscription { PlanCode = s.PlanCode, Plan = s.Plan })
                .FirstOrDefault();

            if (database == null || subscription == null) return null;
            Db.SetServer(database.DatabaseName, database.ServerName);

            return new StoreInfo
            {
                Code = company.CompanyCode,
                Name = company.CompanyName,
                Branch = "Main branch",
                Plan = subscription.PlanCode,
                PlanName = subscription.Plan?.PlanName ?? "",
                Database = database.DatabaseName,
                Status = company.IsActive ? "Active" : "Suspended"
            };
        }

        // The Super Admin is a user in AspNetUsers (from IdentityDbContext).
        // Identity's PasswordHasher checks the password against the stored hash.
        public static bool SuperAdminSignIn(string username, string password)
        {
            using MasterCrmDbContext db = DbContexts.Master();
            string normalized = username.Trim().ToUpperInvariant();
            IdentityUser user = db.Users.SingleOrDefault(u => u.NormalizedUserName == normalized);
            if (user == null || user.PasswordHash == null) return false;

            PasswordVerificationResult result = new PasswordHasher<IdentityUser>().VerifyHashedPassword(user, user.PasswordHash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}

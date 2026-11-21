namespace Xpera.infrastructure.data
{
    // Opens the DbContexts outside ASP.NET. The API gets MasterCrmDbContext from Program.cs (AddDbContext),
    // but the desktop app has no Program.cs services, so it opens them here.
    // The connection comes from Db.Open, which follows the connection rules in Db.cs
    // (LocalDB, or online from databases.txt, and a wrong online password is tried only once).
    public static class DbContexts
    {
        public static MasterCrmDbContext Master()
        {
            return new MasterCrmDbContext(new DbContextOptionsBuilder<MasterCrmDbContext>()
                .UseSqlServer(Db.Open(Db.MasterDatabase), contextOwnsConnection: true).Options);
        }

        // A store's database, like Xpera_TenantA. The signed-in store's database is Session.Store.Database.
        public static TenantCrmDbContext Tenant(string database)
        {
            return new TenantCrmDbContext(new DbContextOptionsBuilder<TenantCrmDbContext>()
                .UseSqlServer(Db.Open(database), contextOwnsConnection: true).Options);
        }
    }

    // Add-Migration / Update-Database do not use this file: they start Xpera.api, whose Program.cs
    // registers both DbContexts with the connection strings in appsettings.json (MasterCrm, TenantA/B/C).
}

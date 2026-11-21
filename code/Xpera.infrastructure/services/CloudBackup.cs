using System.Globalization;
using System.Net.Sockets;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Xpera.domain.entities;
using Xpera.infrastructure.data;

namespace Xpera.infrastructure.services
{
    public enum BackupOutcome { Done, NotSetUp, Offline, WrongPassword, NotUpToDate, Failed }

    public class BackupResult
    {
        public BackupOutcome Outcome { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime When { get; set; } = DateTime.Now;
        public int Rows { get; set; }
        public int Synced { get; set; }      // records that were "not yet synced" and now are
    }

    // What the sidebar shows: the last backup that worked, and the last try if it failed after that.
    public class BackupStatus
    {
        public DateTime? LastSuccess { get; set; }
        public int Rows { get; set; }
        public BackupResult LastFailure { get; set; }
    }

    // Copies one store's LOCAL database up to its CLOUD database (one way: local → cloud).
    // The local database stays the main one; the cloud is the store's backup on MonsterASP.
    //
    // How it finds the cloud database is the teacher's way (Session 3): the cloud Master's CompanyDatabases
    // row for this store → TenantDatabaseResolver → TenantDbContextFactory with the cloud login.
    // The cloud settings (CloudMaster + TenantCredentials) are in a private file on this PC,
    // %USERPROFILE%\.xpera\cloud.json, never in the code.
    public static class CloudBackup
    {
        // XPERA_CLOUD_SETTINGS can point a test copy of the app at another settings file.
        public static readonly string SettingsFile = Environment.GetEnvironmentVariable("XPERA_CLOUD_SETTINGS") ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".xpera", "cloud.json");

        public static bool IsSetUp => File.Exists(SettingsFile);

        public static async Task<BackupResult> RunAsync(StoreInfo store)
        {
            BackupResult result;
            try
            {
                result = await CopyAsync(store);
            }
            catch (SqlException ex) when (ex.Number == 18456)   // login failed
            {
                result = Fail(BackupOutcome.WrongPassword, "The cloud refused the login. Check the cloud passwords in " + SettingsFile + ".");
            }
            catch (SqlException ex) when (IsNetworkError(ex))
            {
                result = Fail(BackupOutcome.Offline, "Can't reach the cloud right now. Check the internet; xpera tries again at the next sign-out.");
            }
            catch (Exception ex)
            {
                result = Fail(BackupOutcome.Failed, "The backup stopped: " + ex.Message);
            }
            SaveStatus(store.Code, result);
            return result;
        }

        // Signing out backs up by itself, except when the last try had a wrong password and cloud.json
        // has not been changed since: MonsterASP blocks a PC after repeated failed logins.
        public static bool ShouldRunOnSignOut(string storeCode)
        {
            if (!IsSetUp) return false;
            BackupResult failure = LoadStatus(storeCode).LastFailure;
            return !(failure != null && failure.Outcome == BackupOutcome.WrongPassword
                     && File.GetLastWriteTime(SettingsFile) <= failure.When);
        }

        private static async Task<BackupResult> CopyAsync(StoreInfo store)
        {
            if (!IsSetUp)
                return Fail(BackupOutcome.NotSetUp, "Cloud backup isn't set up on this PC yet (" + SettingsFile + " is missing).");

            IConfiguration config = new ConfigurationBuilder().AddJsonFile(SettingsFile, optional: false).Build();
            string masterConnection = config.GetConnectionString("CloudMaster");
            if (string.IsNullOrWhiteSpace(masterConnection))
                return Fail(BackupOutcome.NotSetUp, "cloud.json has no CloudMaster connection string.");

            // A quick check first, so signing out without internet doesn't hang for long.
            if (!await CanReachAsync(masterConnection))
                return Fail(BackupOutcome.Offline, "Can't reach the cloud right now. Check the internet; xpera tries again at the next sign-out.");

            // 1. The cloud Master knows which cloud database is this store's.
            await using MasterCrmDbContext master = new MasterCrmDbContext(
                new DbContextOptionsBuilder<MasterCrmDbContext>().UseSqlServer(masterConnection).Options);
            Company company = await master.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyCode == store.Code);
            if (company == null)
                return Fail(BackupOutcome.Failed, "The cloud Master has no store with the code " + store.Code + ".");

            // 2. The teacher's resolver + factory open that cloud database with the store's cloud login.
            TenantDbContextFactory factory = new TenantDbContextFactory(new TenantDatabaseResolver(master), config);
            await using TenantCrmDbContext cloud = await factory.CreateAsync(company.CompanyId);
            await using TenantCrmDbContext local = DbContexts.Tenant(store.Database);

            // Open both connections ourselves: one login try each. (EF's own migration check retries a failed
            // login for about a minute, which on MonsterASP would count as many wrong passwords.)
            SqlConnection localSql = (SqlConnection)local.Database.GetDbConnection();
            SqlConnection cloudSql = (SqlConnection)cloud.Database.GetDbConnection();
            if (localSql.State != System.Data.ConnectionState.Open) await localSql.OpenAsync();
            if (cloudSql.State != System.Data.ConnectionState.Open) await cloudSql.OpenAsync();

            // 3. Both must have the same tables (the same last migration), or the copy would not fit.
            string localLast = await LastMigrationAsync(localSql);
            string cloudLast = await LastMigrationAsync(cloudSql);
            if (localLast != cloudLast)
                return Fail(BackupOutcome.NotUpToDate, "The cloud database doesn't have the newest tables yet (" + (cloudLast ?? "none") +
                            "). Run Update-Database for this store's cloud database first.");

            // 4. Mark this PC's records as synced, inside a LOCAL transaction that stays open until the cloud has them.
            //    Serializable = nobody can add or change a row while it is being copied, so a sale saved during the
            //    backup can't be marked "synced" by mistake (it waits a moment and stays "not yet synced").
            //    Marking first also means the cloud copy says IsSynced = 1. If the cloud copy fails, this is undone.
            await using SqlTransaction localTransaction = (SqlTransaction)await localSql.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            int synced = 0;
            foreach (string table in SyncTrackedTables(local))
            {
                using SqlCommand mark = new SqlCommand("UPDATE [" + table + "] SET IsSynced = 1 WHERE IsSynced = 0", localSql, localTransaction);
                synced += await mark.ExecuteNonQueryAsync();
            }

            // 5. Replace the cloud copy in one transaction: all of it is copied, or nothing changes.
            //    Children are emptied first and parents filled first, so the foreign keys always hold.
            List<string> tables = TablesInOrder(local);
            int rows = 0;
            await using SqlTransaction transaction = (SqlTransaction)await cloudSql.BeginTransactionAsync();

            foreach (string table in Enumerable.Reverse(tables))
            {
                using SqlCommand empty = new SqlCommand("DELETE FROM [" + table + "]", cloudSql, transaction);
                await empty.ExecuteNonQueryAsync();
            }

            foreach (string table in tables)
            {
                using (SqlCommand count = new SqlCommand("SELECT COUNT(*) FROM [" + table + "]", localSql, localTransaction))
                    rows += (int)await count.ExecuteScalarAsync();

                using SqlCommand read = new SqlCommand("SELECT * FROM [" + table + "]", localSql, localTransaction);
                await using SqlDataReader reader = await read.ExecuteReaderAsync();

                // KeepIdentity keeps the same IDs, so receipt OR-000483 is sale 412 in both places.
                using SqlBulkCopy copy = new SqlBulkCopy(cloudSql,
                    SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.CheckConstraints, transaction)
                {
                    DestinationTableName = "[" + table + "]",
                    BulkCopyTimeout = 120
                };
                for (int i = 0; i < reader.FieldCount; i++)
                    copy.ColumnMappings.Add(reader.GetName(i), reader.GetName(i));
                await copy.WriteToServerAsync(reader);

                // The next new row in the cloud continues after the copied IDs.
                using SqlCommand reseed = new SqlCommand(
                    "IF OBJECTPROPERTY(OBJECT_ID(@t), 'TableHasIdentity') = 1 DBCC CHECKIDENT (@t) WITH NO_INFOMSGS;", cloudSql, transaction);
                reseed.Parameters.AddWithValue("@t", table);
                await reseed.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            await localTransaction.CommitAsync();    // only now do the records on this PC say "synced"
            return new BackupResult
            {
                Outcome = BackupOutcome.Done,
                Rows = rows,
                Synced = synced,
                Message = store.Name + "'s data is backed up to the cloud: " + rows.ToString("N0") + " rows in " + tables.Count + " tables" +
                          (synced == 0 ? ", nothing new since the last backup." : ", " + synced.ToString("N0") + " new or changed records now synced.")
            };
        }

        // The tables with an IsSynced column (entities that are ISyncTracked), like Customers and Sales.
        private static List<string> SyncTrackedTables(DbContext db)
        {
            return db.Model.GetEntityTypes()
                .Where(e => typeof(ISyncTracked).IsAssignableFrom(e.ClrType) && e.GetTableName() != null)
                .Select(e => e.GetTableName()).Distinct().ToList();
        }

        // Every table the DbContext knows, parents before children (Customers before Sales, Sales before SaleItems),
        // worked out from the foreign keys. A table added in Lab 2 is included by itself.
        private static List<string> TablesInOrder(DbContext db)
        {
            List<IEntityType> left = db.Model.GetEntityTypes().Where(e => e.GetTableName() != null && e.GetViewName() == null).ToList();
            List<IEntityType> ordered = new List<IEntityType>();
            while (left.Count > 0)
            {
                IEntityType next = left.FirstOrDefault(e => e.GetForeignKeys()
                    .All(fk => fk.PrincipalEntityType == e || ordered.Contains(fk.PrincipalEntityType)));
                if (next == null) throw new InvalidOperationException("The tables point at each other in a circle, so there is no safe copy order.");
                ordered.Add(next);
                left.Remove(next);
            }
            return ordered.Select(e => e.GetTableName()).Distinct().ToList();
        }

        // The newest row in __EFMigrationsHistory, or null when the database has no migrations yet.
        private static async Task<string> LastMigrationAsync(SqlConnection sql)
        {
            using SqlCommand command = new SqlCommand(
                "IF OBJECT_ID('__EFMigrationsHistory') IS NULL SELECT CAST(NULL AS nvarchar(150)) ELSE SELECT MAX(MigrationId) FROM __EFMigrationsHistory", sql);
            object value = await command.ExecuteScalarAsync();
            return value == DBNull.Value ? null : (string)value;
        }

        private static async Task<bool> CanReachAsync(string connectionString)
        {
            string source = new SqlConnectionStringBuilder(connectionString).DataSource;
            if (source.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase) || source.StartsWith(".") ||
                source.StartsWith("localhost", StringComparison.OrdinalIgnoreCase)) return true;

            string host = source.Replace("tcp:", "", StringComparison.OrdinalIgnoreCase);
            int port = 1433;
            int comma = host.IndexOf(',');
            if (comma >= 0) { int.TryParse(host.Substring(comma + 1), out port); host = host.Substring(0, comma); }
            try
            {
                using TcpClient client = new TcpClient();
                Task connect = client.ConnectAsync(host.Trim(), port);
                return await Task.WhenAny(connect, Task.Delay(4000)) == connect && !connect.IsFaulted;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private static bool IsNetworkError(SqlException ex)
        {
            // -2 timeout, 53/40 server not found, 10053/10054/10060 connection dropped, 11001 no such host
            return new[] { -2, 40, 53, 121, 1231, 10053, 10054, 10060, 11001 }.Contains(ex.Number);
        }

        private static BackupResult Fail(BackupOutcome outcome, string message)
        {
            return new BackupResult { Outcome = outcome, Message = message };
        }

        // ---------- when the last backup was (kept on this PC, one small file per store) ----------

        private static string StatusFile(string storeCode)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Xpera", "backup-" + storeCode.ToUpperInvariant() + ".txt");
        }

        public static BackupStatus LoadStatus(string storeCode)
        {
            BackupStatus status = new BackupStatus();
            try
            {
                if (!File.Exists(StatusFile(storeCode))) return status;
                foreach (string line in File.ReadAllLines(StatusFile(storeCode)))
                {
                    string[] part = line.Split('|');
                    if (part[0] == "ok" && part.Length >= 3)
                    {
                        status.LastSuccess = DateTime.Parse(part[1], CultureInfo.InvariantCulture);
                        status.Rows = int.Parse(part[2], CultureInfo.InvariantCulture);
                    }
                    else if (part[0] == "failed" && part.Length >= 4)
                    {
                        status.LastFailure = new BackupResult
                        {
                            When = DateTime.Parse(part[1], CultureInfo.InvariantCulture),
                            Outcome = Enum.Parse<BackupOutcome>(part[2]),
                            Message = part[3]
                        };
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is FormatException || ex is ArgumentException)
            {
                // A damaged status file only means "no backup known yet".
            }
            return status;
        }

        private static void SaveStatus(string storeCode, BackupResult result)
        {
            try
            {
                BackupStatus status = LoadStatus(storeCode);
                List<string> lines = new List<string>();
                if (result.Outcome == BackupOutcome.Done)
                {
                    lines.Add("ok|" + result.When.ToString("o", CultureInfo.InvariantCulture) + "|" + result.Rows);
                }
                else
                {
                    if (status.LastSuccess != null)
                        lines.Add("ok|" + status.LastSuccess.Value.ToString("o", CultureInfo.InvariantCulture) + "|" + status.Rows);
                    lines.Add("failed|" + result.When.ToString("o", CultureInfo.InvariantCulture) + "|" + result.Outcome + "|" + result.Message.Replace("|", "/").Replace("\n", " "));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(StatusFile(storeCode)));
                File.WriteAllLines(StatusFile(storeCode), lines);
            }
            catch (IOException)
            {
                // Not remembering the time is fine; the backup itself already happened.
            }
        }
    }
}

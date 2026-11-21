using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Xpera.infrastructure.data
{
    // The connection rules: which server and database to open, and how passwords are hashed.
    // The queries themselves are in the DbContexts (MasterCrmDbContext, TenantCrmDbContext); EF Core always
    // sends values as parameters, so a typed apostrophe or a trick like "'; DROP TABLE" is harmless.
    public static class Db
    {
        // Without a settings file, every database lives in LocalDB on this PC (it comes with Visual Studio).
        // XPERA_LOCAL_SERVER can point a test copy of the app at another LocalDB instance, like (localdb)\XperaTest.
        public static readonly string LocalServer = Environment.GetEnvironmentVariable("XPERA_LOCAL_SERVER") ?? @"(localdb)\MSSQLLocalDB";
        public const string MasterDatabase = "Xpera_Master";

        // The online databases (MonsterASP) are listed in this file on the PC, never in the code,
        // because each line holds a password. One line per database: Xpera_Master = <connection string>.
        // It sits in the user's folder (C:\Users\<name>\.xpera), outside AppData, so any program can find it.
        public static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".xpera", "databases.txt");

        private static Dictionary<string, string> _online;

        // Which server each store database lives on, from CompanyDatabases.ServerName in the Master.
        // A database not listed here is on LocalServer.
        private static readonly Dictionary<string, string> _servers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void SetServer(string database, string server)
        {
            if (!string.IsNullOrWhiteSpace(server)) _servers[database] = server;
        }

        // A database whose password was wrong is not tried again until the app restarts.
        // MonsterASP blocks a PC after repeated failed logins, so one wrong try must stay one.
        private static readonly Dictionary<string, SqlException> _wrongLogin = new Dictionary<string, SqlException>();

        public static SqlConnection Open(string database)
        {
            if (_wrongLogin.TryGetValue(database, out SqlException earlier)) throw earlier;

            SqlConnection sql = new SqlConnection(ConnectionString(database));
            try
            {
                sql.Open();
            }
            catch (SqlException ex) when (ex.Number == 18456)   // 18456 = login failed
            {
                _wrongLogin[database] = ex;
                sql.Dispose();
                throw;
            }
            return sql;
        }

        // Online if the settings file has a line for this database, otherwise the server from the Master
        // (LocalDB on this PC). XPERA_LOCAL_SERVER still wins over LocalDB, for testing.
        public static string ConnectionString(string database)
        {
            if (_online == null) _online = LoadSettings();
            if (_online.TryGetValue(database, out string online))
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(online) { ConnectTimeout = 20 };
                return builder.ConnectionString;
            }
            string server = _servers.TryGetValue(database, out string listed) && !listed.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
                ? listed : LocalServer;
            return "Server=" + server + ";Database=" + database +
                ";Integrated Security=true;TrustServerCertificate=true;Connect Timeout=15";
        }

        // Skips comments, empty lines, and lines whose password is still ********.
        private static Dictionary<string, string> LoadSettings()
        {
            Dictionary<string, string> online = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(SettingsFile)) return online;
            foreach (string line in File.ReadAllLines(SettingsFile))
            {
                int equals = line.IndexOf('=');
                if (line.TrimStart().StartsWith("#") || equals < 0) continue;
                string name = line.Substring(0, equals).Trim();
                string connection = line.Substring(equals + 1).Trim();
                if (connection == "" || connection.Contains("********")) continue;
                online[name] = connection;
            }
            return online;
        }

        // The same hash the SQL scripts make: SHA-256 of "username:password".
        // The database only ever keeps this hash, never the password itself.
        public static byte[] HashPassword(string username, string password)
        {
            return SHA256.HashData(Encoding.Unicode.GetBytes(username.Trim().ToLower() + ":" + password));
        }
    }
}

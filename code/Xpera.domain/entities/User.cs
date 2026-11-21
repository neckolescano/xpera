using System.Text.Json.Serialization;

namespace Xpera.domain.entities
{
    // UC-01: a staff account in one store (table Users).
    public class User : ISyncTracked
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;   // stored in lowercase
        public string Role { get; set; } = string.Empty;       // Admin, Cashier, or Staff
        public string Status { get; set; } = "Active";         // Active or Locked
        public int FailedLogins { get; set; }                  // the account locks after 3 wrong passwords

        // SHA-256 of "username:password", never the password itself. [JsonIgnore] keeps it out of API answers.
        [JsonIgnore]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public bool IsSynced { get; set; }                     // false = changed since the last cloud backup
    }
}

namespace Xpera.domain.entities
{
    // The Master's "phone book" of usernames (table CompanyUsers): which store each username belongs to.
    // Sign-in only asks for a username and password, so the Master looks the username up here,
    // finds the company, and from CompanyDatabases knows which store database to open.
    //
    // Only the username and the company are kept here. The password, role, and lock status stay in the
    // store's own Users table, so each fact is stored once (and the Master never holds store passwords).
    // A username can be used by only one person in all of xpera (unique), or sign-in couldn't tell stores apart.
    public class CompanyUser
    {
        public int CompanyUserId { get; set; }

        public int CompanyId { get; set; }

        public string Username { get; set; } = string.Empty;   // lowercase, like cashier01

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Company? Company { get; set; }
    }
}

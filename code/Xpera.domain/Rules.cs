using System.Text.RegularExpressions;

namespace Xpera.domain
{
    // The input rules, in one place, so the app's forms and the API check the same things.
    // Each Check... returns the problem as a sentence, or null when the value is fine.
    // The lengths match the table columns (TenantCrmDbContext), so nothing typed is ever too long to save.
    public static class Rules
    {
        // Column sizes
        public const int NameLength = 80;          // Users/Customers.FullName, Products.Name, Customers.UsuallyBuys
        public const int UsernameLength = 40;      // Users.Username
        public const int MobileLength = 11;        // 09XXXXXXXXX
        public const int QuestionLength = 200;     // Inquiries.Question, Complaints.Details
        public const int CommentLength = 300;      // Feedback.Comment
        public const int PasswordMin = 6;
        public const int PasswordMax = 50;
        public const decimal MaxMoney = 99_999_999.99m;   // decimal(10,2)

        // ---------- mobile numbers (Philippines): 11 digits, starts with 09, like 09565634228 ----------

        public static string? CheckMobile(string? text)
        {
            string digits = Digits(text);
            if (digits.Length != MobileLength || !digits.StartsWith("09"))
                return "The mobile number must be 11 digits and start with 09, like 09565634228.";
            return null;
        }

        // Saved the same way as the other customers: 0956 563 4228.
        public static string FormatMobile(string? text)
        {
            string d = Digits(text);
            return d.Length == MobileLength ? d.Substring(0, 4) + " " + d.Substring(4, 3) + " " + d.Substring(7) : (text ?? "").Trim();
        }

        public static string Digits(string? text)
        {
            return new string((text ?? "").Where(char.IsDigit).ToArray());
        }

        // ---------- names: letters, spaces, and . - ' (like Ma. Clara Dela Cruz-Reyes, O'Neil) ----------

        public static string? CheckPersonName(string? text)
        {
            text = (text ?? "").Trim();
            if (text.Length < 2 || !text.Any(char.IsLetter)) return "Type the full name, like Marco Reyes.";
            if (text.Length > NameLength) return "The name can have up to " + NameLength + " characters.";
            if (!text.All(c => char.IsLetter(c) || c == ' ' || c == '.' || c == '-' || c == '\''))
                return "A name can only have letters, spaces, and . - '";
            return null;
        }

        public static bool IsNameChar(char c)
        {
            return char.IsLetter(c) || c == ' ' || c == '.' || c == '-' || c == '\'';
        }

        // ---------- usernames: 3 to 40 lowercase letters, numbers, . or _, starting with a letter ----------

        public static string? CheckUsername(string? text)
        {
            text = (text ?? "").Trim();
            if (!Regex.IsMatch(text, "^[a-z][a-z0-9._]{2,39}$"))
                return "A username has 3 to 40 small letters, numbers, . or _ and starts with a letter.";
            return null;
        }

        public static bool IsUsernameChar(char c)
        {
            return (c >= 'a' && c <= 'z') || char.IsDigit(c) || c == '.' || c == '_';
        }

        public static string? CheckPassword(string? text)
        {
            if ((text ?? "").Length < PasswordMin) return "The password needs at least " + PasswordMin + " characters.";
            if (text!.Length > PasswordMax) return "The password can have up to " + PasswordMax + " characters.";
            return null;
        }

        // ---------- free text, like a question, a complaint, or a product name ----------

        public static string? CheckText(string label, string? text, int max, int min = 2)
        {
            text = (text ?? "").Trim();
            if (text.Length < min) return label + " needs at least " + min + " characters.";
            if (text.Length > max) return label + " can have up to " + max + " characters.";
            return null;
        }

        // ---------- money: more than ₱0, up to what the database column holds ----------

        public static string? CheckMoney(string label, decimal amount)
        {
            if (amount <= 0) return label + " must be more than ₱0.";
            if (amount > MaxMoney) return label + " is too big.";
            return null;
        }
    }
}

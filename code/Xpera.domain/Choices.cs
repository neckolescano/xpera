namespace Xpera.domain
{
    // The fixed options in dropdowns. They match the CHECK rules in the database tables.
    public static class Choices
    {
        public static readonly string[] Roles = { "Admin", "Cashier", "Staff" };
        public static readonly string[] Categories = { "Graphics cards", "Processors", "Memory", "Storage", "Monitors", "Peripherals" };
        public static readonly string[] Availability = { "Available", "Not available" };
        public static readonly string[] InquiryStatus = { "Open", "In progress", "Waiting for product", "Resolved" };
        public static readonly string[] ComplaintStatus = { "Open", "In progress", "Resolved" };
        public static readonly string[] ComplaintTypes = { "Complaint", "Warranty claim" };
    }
}

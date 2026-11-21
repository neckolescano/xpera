using Xpera.Controls;

namespace Xpera.Screens
{
    // Shared rows for dropdowns, so every form shows customers the same way: initials, what they usually buy, and their number.
    public static class Pick
    {
        public static List<ChoiceBox.Choice> Customers(List<(int Id, string Name)> customers)
        {
            Dictionary<int, object[]> details = StoreDb.Customers().ToDictionary(c => c.Index, c => c.Cells);
            return customers.Select(c => new ChoiceBox.Choice
            {
                Title = c.Name,
                Badge = Initials(c.Name),
                Detail = details.TryGetValue(c.Id, out object[] cells) ? cells[2] + "  ·  " + cells[1] : ""
            }).ToList();
        }

        public static string Initials(string name)
        {
            return string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => char.IsLetterOrDigit(w[0])).Take(2).Select(w => char.ToUpper(w[0])));
        }
    }
}

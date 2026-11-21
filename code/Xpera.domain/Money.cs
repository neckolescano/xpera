namespace Xpera.domain
{
    // How the app writes pesos, like ₱45,200. The screens and the database code both use it.
    public static class Money
    {
        public static string Peso(decimal amount)
        {
            return "₱" + amount.ToString("#,##0");
        }

        public static decimal ParsePeso(string text)
        {
            decimal.TryParse(text.Replace("₱", "").Replace(",", ""), out decimal amount);
            return amount;
        }
    }
}

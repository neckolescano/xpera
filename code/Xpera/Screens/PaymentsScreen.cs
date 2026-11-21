using Xpera.Controls;

namespace Xpera.Screens
{
    // Balances from credit sales, and the payments that lower them.
    public partial class PaymentsScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public PaymentsScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            Ui.ShowAsTags(grid, "statusColumn");
            _list = new PagedList(grid, pager, countLabel, "balances", RowsWithStats);
            Load += (sender, e) => _list.Refresh();
            VisibleChanged += (sender, e) => { if (Visible) _list.Refresh(); };
        }

        // The + Add button goes in the header, on the right of the title.
        public Control[] HeaderActions()
        {
            return new Control[] { addButton };
        }

        // The list asks for its rows each time it refreshes, so the tiles stay up to date too.
        private List<(int Index, object[] Cells)> RowsWithStats()
        {
            List<(int Index, object[] Cells)> rows = Rows();
            ShowStats(rows);
            return rows;
        }

        private void ShowStats(List<(int Index, object[] Cells)> rows)
        {
            decimal Owed((int Index, object[] Cells) r) => Ui.ParsePeso(Convert.ToString(r.Cells[1]));
            var overdue = rows.Where(r => Convert.ToString(r.Cells[3]) == "Overdue").ToList();
            int soon = rows.Count(r => Convert.ToString(r.Cells[3]) == "Due soon");
            stats.SetTiles(
                StatStrip.Make("Total owed", Ui.Peso(rows.Sum(Owed)), "from credit sales"),
                StatStrip.Make("Overdue", overdue.Count.ToString(), overdue.Count == 0 ? "nothing is late" : Ui.Peso(overdue.Sum(Owed)) + " past due", overdue.Count > 0),
                StatStrip.Make("Due this week", soon.ToString(), "remind them before it's late"),
                StatStrip.Make("Customers owing", rows.Count.ToString(), "with a balance"));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Balances();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            List<(int Index, object[] Cells)> owing = StoreDb.Balances();
            if (owing.Count == 0)
            {
                Notice.Show(this, "Nothing to collect", "No one owes anything right now.", NoticeKind.Success);
                return;
            }

            // Each customer shows what they owe on the right, and the line under the field says when it is due.
            EntryForm form = new EntryForm("Record payment",
                "The payment comes off the customer's balance from credit sales, oldest sale first.", "Record payment");
            form.AddChoiceField("Customer", owing.Select(b => new ChoiceBox.Choice
            {
                Title = Convert.ToString(b.Cells[0]), Side = Convert.ToString(b.Cells[1]),
                Detail = "Due " + b.Cells[2] + "  ·  " + b.Cells[3], Badge = Pick.Initials(Convert.ToString(b.Cells[0]))
            }), "Search customers who owe");
            form.SetNote(0, i => i < 0 ? "" : "Owes " + owing[i].Cells[1] + "  ·  due " + owing[i].Cells[2] + "  ·  " + Convert.ToString(owing[i].Cells[3]).ToLower());
            form.AddMoneyField("Amount paid", "Part or all of the balance");
            form.Check = values =>
            {
                decimal owed = Ui.ParsePeso(Convert.ToString(owing[form.ChoiceIndex(0)].Cells[1]));
                decimal amount = Ui.ParsePeso(values[1]);
                return amount <= 0 || amount > owed ? "Type an amount from ₱1 to " + Ui.Peso(owed) + "." : null;
            };
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.RecordPayment(owing[form.ChoiceIndex(0)].Index, Ui.ParsePeso(form.Values[1]));
            _list.Refresh();
        }
    }
}

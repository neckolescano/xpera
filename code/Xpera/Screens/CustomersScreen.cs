using Xpera.Controls;

namespace Xpera.Screens
{
    // Every customer the store knows. Sales, concerns, and feedback are tied to them.
    public partial class CustomersScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public CustomersScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            _list = new PagedList(grid, pager, countLabel, "customers", RowsWithStats);
            Ui.AddRowActions(grid, id => new (string, Action)[] { ("Edit customer", () => EditCustomer(id)), ("Delete", () => DeleteCustomer(id)) });
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
            decimal Spent((int Index, object[] Cells) r) => Ui.ParsePeso(Convert.ToString(r.Cells[3]));
            decimal total = rows.Sum(Spent);
            var top = rows.OrderByDescending(Spent).FirstOrDefault();
            int never = rows.Count(r => Convert.ToString(r.Cells[4]) == "—");
            stats.SetTiles(
                StatStrip.Make("Customers", rows.Count.ToString(), never == 0 ? "every one has bought" : never + " have not bought yet"),
                StatStrip.Make("Total spent", Ui.Peso(total), "across every sale"),
                StatStrip.Make("Average spend", Ui.Peso(rows.Count == 0 ? 0 : total / rows.Count), "per customer"),
                StatStrip.Make("Top customer", top.Cells == null ? "—" : Convert.ToString(top.Cells[0]), top.Cells == null ? "" : Ui.Peso(Spent(top)) + " spent"));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Customers();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Add customer", "Every sale, concern, and feedback is saved under their name.", "Add customer");
            form.AddNameField("Full name", "Like Marco Reyes");
            form.AddPhoneField("Mobile number", "Like 09565634228");
            form.AddTextField("Usually buys", "Like GPUs, cooling", Rules.NameLength);
            // One customer per number, so the same person isn't added twice.
            form.Check = values => StoreDb.ContactTaken(Rules.FormatMobile(values[1])) ? "A customer already has this mobile number." : null;
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.AddCustomer(form.Values[0], Rules.FormatMobile(form.Values[1]), form.Values[2]);
            _list.Refresh();
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        private void EditCustomer(int id)
        {
            Customer customer = StoreDb.GetCustomer(id);
            if (customer == null) return;
            EntryForm form = new EntryForm("Edit customer", "Changes show everywhere this customer appears.", "Save changes");
            form.AddNameField("Full name", "Like Marco Reyes");
            form.AddPhoneField("Mobile number", "Like 09565634228");
            form.AddTextField("Usually buys", "Like GPUs, cooling", Rules.NameLength);
            form.SetValue(0, customer.FullName);
            form.SetValue(1, customer.Contact);
            form.SetValue(2, customer.UsuallyBuys);
            form.Check = values => StoreDb.ContactTaken(Rules.FormatMobile(values[1]), id) ? "Another customer already has this mobile number." : null;
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.UpdateCustomer(id, form.Values[0], Rules.FormatMobile(form.Values[1]), form.Values[2]);
            _list.Refresh();
        }

        private void DeleteCustomer(int id)
        {
            Customer customer = StoreDb.GetCustomer(id);
            if (customer == null) return;
            ConfirmDelete("customer", customer.FullName, () => StoreDb.DeleteCustomer(id, onlyCheck: true), () => StoreDb.DeleteCustomer(id));
        }

        // Asks "are you sure?" unless the record can't be deleted at all, then deletes it.
        // check / delete come from StoreDb: they return the reason it can't be deleted, or null.
        private void ConfirmDelete(string what, string name, Func<string> check, Action delete)
        {
            string problem = check();
            if (problem != null)
            {
                Notice.Show(this, "Can't delete", problem, NoticeKind.Warning);
                return;
            }
            if (!Notice.Confirm(this, "Delete " + what + "?", name + " will be removed. This can't be undone.", "Delete")) return;
            delete();
            _list.Refresh();
        }
    }
}

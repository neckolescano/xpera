using Xpera.Controls;

namespace Xpera.Screens
{
    // Customer questions. If a product is not available, the inquiry waits until it comes back.
    public partial class InquiriesScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public InquiriesScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            Ui.MakeDropdown(grid, "statusColumn", Choices.InquiryStatus, StoreDb.SetInquiryStatus);
            _list = new PagedList(grid, pager, countLabel, "inquiries", RowsWithStats);
            Ui.AddRowActions(grid, id => new (string, Action)[] { ("Edit inquiry", () => EditInquiry(id)), ("Delete", () => DeleteInquiry(id)) });
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
            int Count(string status) => rows.Count(r => Convert.ToString(r.Cells[4]) == status);
            int waiting = Count("Waiting for product");
            stats.SetTiles(
                StatStrip.Make("Open", Count("Open").ToString(), "not answered yet", Count("Open") > 0),
                StatStrip.Make("In progress", Count("In progress").ToString(), "being worked on"),
                StatStrip.Make("Waiting for product", waiting.ToString(), "follow up when it's back"),
                StatStrip.Make("Resolved", Count("Resolved").ToString(), "of " + rows.Count + " inquiries"));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Inquiries();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Log inquiry", "If they ask about a product that is not available, the inquiry waits until it is back.", "Log inquiry");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddTextField("Inquiry", "What did they ask?", Rules.QuestionLength);
            List<(int Id, string Name, decimal Price, bool Available)> products = StoreDb.ProductChoices();
            form.AddChoiceField("Product (if they asked about one)",
                new[] { new ChoiceBox.Choice { Title = "—", Detail = "Not about a product" } }
                    .Concat(products.Select(p => new ChoiceBox.Choice { Title = p.Name, Detail = p.Available ? "Available" : "Not available, the inquiry will wait", Side = Ui.Peso(p.Price) })),
                "Search products");
            if (form.ShowModal(this) != DialogResult.OK) return;

            // A product that is not available keeps the inquiry waiting (the database decides this).
            int picked = form.ChoiceIndex(2);
            int? productId = picked > 0 ? products[picked - 1].Id : (int?)null;
            StoreDb.AddInquiry(customers[form.ChoiceIndex(0)].Id, form.Values[1], productId);
            _list.Refresh();
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        private void EditInquiry(int id)
        {
            Inquiry inquiry = StoreDb.GetInquiry(id);
            if (inquiry == null) return;
            EntryForm form = new EntryForm("Edit inquiry", "Change who asked, the question, or the product. The status is changed in the list.", "Save changes");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddTextField("Inquiry", "What did they ask?", Rules.QuestionLength);
            List<(int Id, string Name, decimal Price, bool Available)> products = StoreDb.ProductChoices();
            form.AddChoiceField("Product (if they asked about one)",
                new[] { new ChoiceBox.Choice { Title = "—", Detail = "Not about a product" } }
                    .Concat(products.Select(p => new ChoiceBox.Choice { Title = p.Name, Detail = p.Available ? "Available" : "Not available", Side = Ui.Peso(p.Price) })),
                "Search products");
            form.SetChoice(0, customers.FindIndex(c => c.Id == inquiry.CustomerId));
            form.SetValue(1, inquiry.Question);
            form.SetChoice(2, inquiry.ProductId == null ? 0 : products.FindIndex(p => p.Id == inquiry.ProductId) + 1);
            if (form.ShowModal(this) != DialogResult.OK) return;

            int picked = form.ChoiceIndex(2);
            StoreDb.UpdateInquiry(id, customers[form.ChoiceIndex(0)].Id, form.Values[1], picked > 0 ? products[picked - 1].Id : (int?)null);
            _list.Refresh();
        }

        private void DeleteInquiry(int id)
        {
            Inquiry inquiry = StoreDb.GetInquiry(id);
            if (inquiry == null) return;
            ConfirmDelete("inquiry", "\"" + inquiry.Question + "\"", () => null, () => StoreDb.DeleteInquiry(id));
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

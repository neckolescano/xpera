using Xpera.Controls;

namespace Xpera.Screens
{
    // What the store sells. Availability only, no stock counts.
    public partial class ProductsScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        // UC-02: Staff only updates availability; the Admin manages the list (add, edit, delete).
        private static bool IsAdmin => (Session.Role ?? "Admin") == "Admin";

        public ProductsScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            Ui.MakeDropdown(grid, "availabilityColumn", Choices.Availability, AvailabilityChanged);
            _list = new PagedList(grid, pager, countLabel, "products", RowsWithStats);
            if (IsAdmin)
                Ui.AddRowActions(grid, id => new (string, Action)[] { ("Edit product", () => EditProduct(id)), ("Delete", () => DeleteProduct(id)) });
            else
                grid.Columns["actionsColumn"].Visible = false;
            Load += (sender, e) => _list.Refresh();
            VisibleChanged += (sender, e) => { if (Visible) _list.Refresh(); };
        }

        // The + Add button goes in the header, on the right of the title.
        public Control[] HeaderActions()
        {
            return IsAdmin ? new Control[] { addButton } : new Control[0];
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
            int off = rows.Count(r => Convert.ToString(r.Cells[3]) == "Not available");
            int waiting = rows.Sum(r => int.TryParse(Convert.ToString(r.Cells[4]).Split(' ')[0], out int n) ? n : 0);
            stats.SetTiles(
                StatStrip.Make("Products", rows.Count.ToString(), rows.Select(r => r.Cells[1]).Distinct().Count() + " categories"),
                StatStrip.Make("Available", (rows.Count - off).ToString(), "ready to sell"),
                StatStrip.Make("Not available", off.ToString(), off == 0 ? "everything is in" : "sold out for now", off > 0),
                StatStrip.Make("Customers waiting", waiting.ToString(), waiting == 0 ? "no one is waiting" : "call them when it's back", waiting > 0));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Products();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Add product", "New products start as Available. Staff can change that in the list.", "Add product");
            form.AddTextField("Product name", "Like RTX 4060 8GB", Rules.NameLength);
            form.AddChoiceField("Category", Choices.Categories);
            form.AddMoneyField("Price", "Like 2990");
            form.Check = values => StoreDb.ProductNameTaken(values[0]) ? "A product with that name is already in the list." : null;
            if (form.ShowModal(this) != DialogResult.OK) return;

            try
            {
                StoreDb.AddProduct(form.Values[0], form.Values[1], Ui.ParsePeso(form.Values[2]));
            }
            catch (Exception ex) when (ex is Microsoft.EntityFrameworkCore.DbUpdateException || ex is Microsoft.Data.SqlClient.SqlException)
            {
                Notice.Show(this, "Already in the list", "A product with that name already exists.", NoticeKind.Warning);
            }
            _list.Refresh();
        }

        private void AvailabilityChanged(int productId, string value)
        {
            int waiting = StoreDb.SetAvailability(productId, value == "Available");
            if (value == "Available" && waiting > 0)
            {
                Notice.Show(this, "Available again", waiting + (waiting == 1 ? " customer is" : " customers are") + " waiting for it. Follow up from Inquiries.", NoticeKind.Success);
            }
            BeginInvoke(new Action(_list.Refresh));
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        private void EditProduct(int id)
        {
            Product product = StoreDb.GetProduct(id);
            if (product == null) return;
            EntryForm form = new EntryForm("Edit product", "The new price is used for new sales. Old sales keep the price they were sold at.", "Save changes");
            form.AddTextField("Product name", "Like RTX 4060 8GB", Rules.NameLength);
            form.AddChoiceField("Category", Choices.Categories);
            form.AddMoneyField("Price", "Like 2990");
            form.SetValue(0, product.Name);
            form.SetChoice(1, Array.IndexOf(Choices.Categories, product.Category));
            form.SetValue(2, product.Price.ToString("0.##"));
            form.Check = values => StoreDb.ProductNameTaken(values[0], id) ? "Another product already has that name." : null;
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.UpdateProduct(id, form.Values[0], form.Values[1], Ui.ParsePeso(form.Values[2]));
            _list.Refresh();
        }

        private void DeleteProduct(int id)
        {
            Product product = StoreDb.GetProduct(id);
            if (product == null) return;
            ConfirmDelete("product", product.Name, () => StoreDb.DeleteProduct(id, onlyCheck: true), () => StoreDb.DeleteProduct(id));
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

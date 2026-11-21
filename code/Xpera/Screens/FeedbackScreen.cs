using Xpera.Controls;

namespace Xpera.Screens
{
    // Ratings and comments after a visit.
    public partial class FeedbackScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public FeedbackScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            _list = new PagedList(grid, pager, countLabel, "entries", RowsWithStats);
            Ui.AddRowActions(grid, id => new (string, Action)[] { ("Edit feedback", () => EditFeedback(id)), ("Delete", () => DeleteFeedback(id)) });
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
            List<int> ratings = rows.Select(r => Convert.ToString(r.Cells[2]).Count(c => c == '★')).ToList();
            int happy = ratings.Count(n => n >= 4), unhappy = ratings.Count(n => n <= 2);
            stats.SetTiles(
                StatStrip.Make("Average rating", ratings.Count == 0 ? "—" : ratings.Average().ToString("0.0") + " / 5", "from " + ratings.Count + " visits"),
                StatStrip.Make("Happy", ratings.Count == 0 ? "—" : (100 * happy / ratings.Count) + "%", "gave 4 or 5 stars"),
                StatStrip.Make("Unhappy", unhappy.ToString(), unhappy == 0 ? "no low ratings" : "gave 1 or 2 stars, reach out", unhappy > 0),
                StatStrip.Make("Latest", ratings.Count == 0 ? "—" : new string('★', ratings[0]), ratings.Count == 0 ? "" : Convert.ToString(rows[0].Cells[1])));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Feedback();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Record feedback", "How did the visit go? Low ratings show up on the Feedback tiles.", "Save feedback");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddChoiceField("Rating", new[] { 5, 4, 3, 2, 1 }.Select(n => new ChoiceBox.Choice
            {
                Title = n.ToString(), Side = new string('★', n) + new string('☆', 5 - n)
            }), "");
            form.AddTextField("Comment", "In their words", Rules.CommentLength);
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.AddFeedback(customers[form.ChoiceIndex(0)].Id, int.Parse(form.Values[1]), form.Values[2]);
            _list.Refresh();
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        private void EditFeedback(int id)
        {
            Feedback feedback = StoreDb.GetFeedback(id);
            if (feedback == null) return;
            EntryForm form = new EntryForm("Edit feedback", "Change who gave it, the rating, or the comment.", "Save changes");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddChoiceField("Rating", new[] { 5, 4, 3, 2, 1 }.Select(n => new ChoiceBox.Choice
            {
                Title = n.ToString(), Side = new string('★', n) + new string('☆', 5 - n)
            }), "");
            form.AddTextField("Comment", "In their words", Rules.CommentLength);
            form.SetChoice(0, customers.FindIndex(c => c.Id == feedback.CustomerId));
            form.SetChoice(1, 5 - feedback.Rating);
            form.SetValue(2, feedback.Comment);
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.UpdateFeedback(id, customers[form.ChoiceIndex(0)].Id, int.Parse(form.Values[1]), form.Values[2]);
            _list.Refresh();
        }

        private void DeleteFeedback(int id)
        {
            Feedback feedback = StoreDb.GetFeedback(id);
            if (feedback == null) return;
            ConfirmDelete("feedback", "This " + feedback.Rating + "-star feedback", () => null, () => StoreDb.DeleteFeedback(id));
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

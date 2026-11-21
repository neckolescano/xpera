using Xpera.Controls;

namespace Xpera.Screens
{
    // Complaints and warranty claims, from Open to Resolved.
    public partial class ComplaintsScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public ComplaintsScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            Ui.MakeDropdown(grid, "statusColumn", Choices.ComplaintStatus, StoreDb.SetComplaintStatus);
            Ui.ShowAsTags(grid, "typeColumn");
            _list = new PagedList(grid, pager, countLabel, "complaints", RowsWithStats);
            Ui.AddRowActions(grid, id => new (string, Action)[] { ("Edit complaint", () => EditComplaint(id)), ("Delete", () => DeleteComplaint(id)) });
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
            stats.SetTiles(
                StatStrip.Make("Open", Count("Open").ToString(), "need a reply", Count("Open") > 0),
                StatStrip.Make("In progress", Count("In progress").ToString(), "being fixed"),
                StatStrip.Make("Resolved", Count("Resolved").ToString(), "of " + rows.Count + " concerns"),
                StatStrip.Make("Warranty claims", rows.Count(r => Convert.ToString(r.Cells[3]) == "Warranty claim").ToString(), "parts sent back"));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Complaints();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Log complaint", "Warranty claims go here too. It starts as Open.", "Log complaint");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddTextField("Complaint", "What went wrong?", Rules.QuestionLength);
            form.AddChoiceField("Type", Choices.ComplaintTypes);
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.AddComplaint(customers[form.ChoiceIndex(0)].Id, form.Values[1], form.Values[2]);
            _list.Refresh();
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        private void EditComplaint(int id)
        {
            Complaint complaint = StoreDb.GetComplaint(id);
            if (complaint == null) return;
            EntryForm form = new EntryForm("Edit complaint", "Change who it's from, what happened, or the type. The status is changed in the list.", "Save changes");
            List<(int Id, string Name)> customers = StoreDb.CustomerChoices();
            form.AddChoiceField("Customer", Pick.Customers(customers), "Search customers");
            form.AddTextField("Complaint", "What went wrong?", Rules.QuestionLength);
            form.AddChoiceField("Type", Choices.ComplaintTypes);
            form.SetChoice(0, customers.FindIndex(c => c.Id == complaint.CustomerId));
            form.SetValue(1, complaint.Details);
            form.SetChoice(2, Array.IndexOf(Choices.ComplaintTypes, complaint.Type));
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.UpdateComplaint(id, customers[form.ChoiceIndex(0)].Id, form.Values[1], form.Values[2]);
            _list.Refresh();
        }

        private void DeleteComplaint(int id)
        {
            Complaint complaint = StoreDb.GetComplaint(id);
            if (complaint == null) return;
            ConfirmDelete(complaint.Type.ToLower(), "\"" + complaint.Details + "\"", () => null, () => StoreDb.DeleteComplaint(id));
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

using Xpera.Controls;

namespace Xpera.Screens
{
    // Staff accounts. The Admin creates them and sets each role.
    public partial class UsersScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public UsersScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(addButton);
            Ui.ShowAsTags(grid, "roleColumn", "statusColumn");
            _list = new PagedList(grid, pager, countLabel, "accounts", RowsWithStats);
            Ui.AddRowActions(grid, UserActions);
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
            int Count(string role) => rows.Count(r => Convert.ToString(r.Cells[2]) == role);
            int locked = rows.Count(r => Convert.ToString(r.Cells[3]) == "Locked");
            stats.SetTiles(
                StatStrip.Make("Accounts", rows.Count.ToString(), "who can sign in"),
                StatStrip.Make("Cashiers", Count("Cashier").ToString(), "record sales and payments"),
                StatStrip.Make("Staff", Count("Staff").ToString(), "handle customer care"),
                StatStrip.Make("Locked", locked.ToString(), locked == 0 ? "no one is locked out" : "can't sign in until unlocked", locked > 0));
        }

        // Every record, in order. The list shows one page of these at a time.
        private List<(int Index, object[] Cells)> Rows()
        {
            return StoreDb.Users();
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            EntryForm form = new EntryForm("Add user", "Give them the temporary password in person. They can sign in right away.", "Add user");
            form.AddNameField("Full name", "Like Alyssa Ramos");
            form.AddUsernameField("Username", "Like cashier02");
            form.AddChoiceField("Role", Choices.Roles);
            form.AddPasswordField("Temporary password");
            form.Check = values => StoreDb.UsernameTaken(values[1]) ? "That username is already taken. Try another one." : null;
            if (form.ShowModal(this) != DialogResult.OK) return;

            try
            {
                StoreDb.AddUser(form.Values[0], form.Values[1], form.Values[2], form.Values[3]);
            }
            catch (Exception ex) when (ex is Microsoft.EntityFrameworkCore.DbUpdateException || ex is Microsoft.Data.SqlClient.SqlException)
            {
                Notice.Show(this, "Username taken", "That username is already taken. Try another one.", NoticeKind.Warning);
            }
            _list.Refresh();
        }

        // ---------- Update and Delete (the U and D of CRUD) ----------

        // A locked account gets Unlock; an active one gets Lock (instead of deleting someone with history).
        private (string, Action)[] UserActions(int id)
        {
            User user = StoreDb.GetUser(id);
            bool locked = user != null && user.Status == "Locked";
            return new (string, Action)[]
            {
                ("Edit account", () => EditUser(id)),
                (locked ? "Unlock" : "Lock", () => SetLocked(id, !locked)),
                ("Reset password", () => ResetPassword(id)),
                ("Delete", () => DeleteUser(id))
            };
        }

        private void EditUser(int id)
        {
            User user = StoreDb.GetUser(id);
            if (user == null) return;
            EntryForm form = new EntryForm("Edit account", "The username stays " + user.Username + ". It's how this person signs in.", "Save changes");
            form.AddNameField("Full name", "Like Alyssa Ramos");
            form.AddChoiceField("Role", Choices.Roles);
            form.SetValue(0, user.FullName);
            form.SetChoice(1, Array.IndexOf(Choices.Roles, user.Role));
            if (form.ShowModal(this) != DialogResult.OK) return;

            string problem = StoreDb.UpdateUser(id, form.Values[0], form.Values[1]);
            if (problem != null) Notice.Show(this, "Not saved", problem, NoticeKind.Warning);
            _list.Refresh();
        }

        private void SetLocked(int id, bool locked)
        {
            User user = StoreDb.GetUser(id);
            if (user == null) return;
            if (locked && !Notice.Confirm(this, "Lock account?", user.FullName + " won't be able to sign in until you unlock the account.", "Lock")) return;
            string problem = StoreDb.SetUserLocked(id, locked);
            if (problem != null) Notice.Show(this, "Can't lock", problem, NoticeKind.Warning);
            else if (!locked) Notice.Show(this, "Unlocked", user.FullName + " can sign in again.", NoticeKind.Success);
            _list.Refresh();
        }

        private void ResetPassword(int id)
        {
            User user = StoreDb.GetUser(id);
            if (user == null) return;
            EntryForm form = new EntryForm("Reset password", "Give " + user.FullName + " the new password in person. It also unlocks wrong-password tries.", "Reset password");
            form.AddPasswordField("New password");
            if (form.ShowModal(this) != DialogResult.OK) return;

            StoreDb.ResetPassword(id, form.Values[0]);
            Notice.Show(this, "Password reset", user.Username + " can sign in with the new password.", NoticeKind.Success);
            _list.Refresh();
        }

        private void DeleteUser(int id)
        {
            User user = StoreDb.GetUser(id);
            if (user == null) return;
            ConfirmDelete("account", user.FullName + " (" + user.Username + ")", () => StoreDb.DeleteUser(id, onlyCheck: true), () => StoreDb.DeleteUser(id));
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

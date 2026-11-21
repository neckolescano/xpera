using Xpera.Controls;
using Xpera.infrastructure.services;

namespace Xpera.Screens
{
    // The Activity log: what is being done in the store, newest first, and whether each record it is about
    // is already in the cloud backup ("Synced to cloud") or only on this PC ("Not yet synced").
    // The status comes from the record's own IsSynced column, so a backup turns the lines to synced by itself.
    public partial class ActivityScreen : UserControl, IHeaderActions
    {
        private readonly PagedList _list;

        public ActivityScreen()
        {
            InitializeComponent();
            Ui.StyleGrid(grid);
            Ui.StylePrimary(backupButton);
            Ui.ShowAsTags(grid, "cloudColumn");
            _list = new PagedList(grid, pager, countLabel, "lines", RowsWithStats);
            Load += (sender, e) => _list.Refresh();
            VisibleChanged += (sender, e) => { if (Visible) _list.Refresh(); };
        }

        // Backing up is the store owner's job (same as the sidebar), so only the Admin gets the button here.
        public Control[] HeaderActions()
        {
            return (Session.Role ?? "Admin") == "Admin" ? new Control[] { backupButton } : new Control[0];
        }

        private List<(int Index, object[] Cells)> RowsWithStats()
        {
            List<(int Index, object[] Cells)> rows = StoreDb.Activity();
            ShowStats(rows);
            return rows;
        }

        private void ShowStats(List<(int Index, object[] Cells)> rows)
        {
            // Counted from the records themselves, not the log lines: one customer added and then edited is 1 record.
            List<(string Table, int Count)> waiting = StoreDb.NotSyncedCounts();
            int notSynced = waiting.Sum(t => t.Count);
            string most = waiting.Count == 0 ? "everything is in the cloud" : "most in " + waiting[0].Table + " (" + waiting[0].Count + ")";

            BackupStatus backup = CloudBackup.LoadStatus(Session.Store?.Code ?? "");
            string today = DateTime.Today.ToString("MMM d");
            int todayLines = rows.Count(r => Convert.ToString(r.Cells[0]).StartsWith(today + ","));

            stats.SetTiles(
                StatStrip.Make("Not yet synced", notSynced.ToString("N0"), notSynced == 0 ? most : "records only on this PC, " + most, notSynced > 0),
                StatStrip.Make("Lines synced", rows.Count(r => Convert.ToString(r.Cells[3]) == "Synced to cloud").ToString("N0"), "records already in the cloud"),
                StatStrip.Make("Last backup", backup.LastSuccess == null ? "—" : backup.LastSuccess.Value.ToString("MMM d"),
                    backup.LastSuccess == null ? "not backed up yet" : backup.LastSuccess.Value.ToString("h:mm tt") + ", " + backup.Rows.ToString("N0") + " rows"),
                StatStrip.Make("Today", todayLines.ToString(), todayLines == 1 ? "thing done today" : "things done today"));
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            _list.Search(searchBox.Text);
        }

        public void Reload()
        {
            _list.Refresh();
        }

        // Same backup as the sidebar button (MainForm reloads this page after it).
        private void backupButton_Click(object sender, EventArgs e)
        {
            if (FindForm() is MainForm main) main.BackUpNow();
        }
    }
}

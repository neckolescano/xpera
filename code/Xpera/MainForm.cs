using Xpera.infrastructure.services;
using Xpera.Screens;
using Xpera.Theme;

namespace Xpera
{
    // The app window: the dark sidebar on the left, and the open screen on the right.
    // The menu is grouped by what the store is doing, and a breadcrumb shows where you are.
    public partial class MainForm : Form
    {
        private readonly Palette _palette = new Palette();
        private Xpera.Controls.MenuButton _activeButton;
        private UserControl _activeScreen;
        private string _storeName = "Gaming Hub";

        public MainForm()
        {
            InitializeComponent();
            ApplyColors();

            // To add a screen to the menu, add one line under its group.
            // Each screen, who can open it, and its group. The menu only shows what the signed-in role does.
            // Screens are made only when they will be shown.
            const string all = "Admin", cashier = "Admin Cashier", staff = "Admin Staff", both = "Admin Cashier Staff";
            AddScreen("Sell", "Sales", cashier, "Pick the customer, add the items, then take cash or credit.", () => new SalesScreen());
            AddScreen("Sell", "Payments", cashier, "Balances from credit sales. Overdue ones come first.", () => new PaymentsScreen());
            AddScreen("Sell", "Products", staff, "What the store sells. Availability only, no stock counts.", () => new ProductsScreen());
            AddScreen("Customer care", "Customers", both, "Every sale, concern, and feedback is tied to a customer.", () => new CustomersScreen());
            AddScreen("Customer care", "Inquiries", staff, "Customer questions. Unavailable products stay open until they come back.", () => new InquiriesScreen());
            AddScreen("Customer care", "Complaints", staff, "Complaints and warranty claims, from Open to Resolved.", () => new ComplaintsScreen());
            AddScreen("Customer care", "Feedback", staff, "Ratings and comments after a visit.", () => new FeedbackScreen());
            AddScreen("Store", "Users", all, "Staff accounts and their roles.", () => new UsersScreen());
            AddScreen("Store", "Activity log", both, "What is being done in the store, and what is already backed up to the cloud.", () => new ActivityScreen());

            // The screen area scrolls up and down when a screen is taller than the window; the sidebar never moves.
            screenPanel.AutoScroll = true;
            screenPanel.Resize += (sender, e) => FitScreen();
            PagedList.ContentHeightChanged += FitScreen;
            WheelScroll wheel = new WheelScroll(this);
            Application.AddMessageFilter(wheel);
            // After signing out this window is gone, so it stops listening (the next sign-in makes a new one).
            FormClosed += (sender, e) => { PagedList.ContentHeightChanged -= FitScreen; Application.RemoveMessageFilter(wheel); };

            // The header buttons sit on the right of the title row.
            contentPanel.Resize += (sender, e) => PlaceActions();
            actionsPanel.SizeChanged += (sender, e) => PlaceActions();

            // Open the first screen once the window is ready.
            Load += (sender, e) => menuPanel.Controls.OfType<Button>().First().PerformClick();
            BuildProfile();
            Resize += (sender, e) => LayoutSidebar();
            LayoutSidebar();
        }

        // The sidebar grows with the window: 248 wide on a laptop (1440 or less), up to 25% bigger on a
        // full HD screen, so it stays about a sixth of the window instead of looking small next to the content.
        // Everything in it (logo, items, icons, text, the card at the bottom) grows by the same amount.
        private void LayoutSidebar()
        {
            // Windows display scaling (125% on many laptops) makes text bigger by itself, but not sizes set in code,
            // so every size here is multiplied by it. The window size is measured at 100% for the same reason.
            float dpi = DeviceDpi / 96f;
            float grow = Math.Clamp(ClientSize.Width / dpi / 1440f, 1f, 1.12f);
            grow = (float)Math.Round(grow * 20) / 20;      // steps of 5%, so small resizes don't redo the layout
            float scale = grow * dpi;
            int S(float value) => (int)Math.Round(value * scale);

            // Text grows less than the boxes around it (Inter already reads large), so labels never get cut off.
            float textScale = 1 + (grow - 1) * 0.4f;
            int pad = S(16);
            int inner = S(248) - pad * 2;
            sidebarPanel.Width = S(248);
            logoMark.SetBounds(S(26), S(30), S(36), S(36));
            logoLabel.Font = Typography.Get(17F * textScale, Weight.SemiBold);
            logoLabel.SetBounds(logoMark.Right + S(10), S(30), S(150), S(36));

            profileCard.SizeScale = scale;
            profileCard.TextScale = textScale;
            themeToggle.SizeScale = scale;
            themeToggle.TextScale = textScale;
            backupRow.SizeScale = scale;
            backupRow.TextScale = textScale;
            profileCard.SetBounds(pad, sidebarPanel.ClientSize.Height - S(16) - S(108), inner, S(108));
            themeToggle.SetBounds(pad, profileCard.Top - S(8) - S(40), inner, S(40));
            backupRow.SetBounds(pad, themeToggle.Top - S(4) - S(52), inner, S(52));
            int lowest = backupRow.Visible ? backupRow.Top : themeToggle.Top;

            // The menu must fit between the logo and the backup row / dark mode switch. On a short window the items
            // and group gaps get a little shorter instead of sliding under them (never below 85%).
            int top = S(100);
            int room = lowest - S(12) - top;
            int units = 0;
            foreach (Control item in menuPanel.Controls)
                units += item is Xpera.Controls.MenuButton ? Sizes.MenuItem + 4 : (menuPanel.Controls.GetChildIndex(item) == 0 ? 22 : 36);
            float tall = Math.Max(0.85f * dpi, Math.Min(scale, units == 0 ? scale : room / (float)units));
            int V(float value) => (int)Math.Round(value * tall);

            menuPanel.SuspendLayout();
            foreach (Control item in menuPanel.Controls)
            {
                if (item is Xpera.Controls.MenuButton button)
                {
                    button.SizeScale = Math.Min(scale, tall);
                    button.Font = Typography.Scaled(Typography.Nav, textScale);
                    button.Size = new Size(inner, V(Sizes.MenuItem));
                    button.Margin = new Padding(0, 0, 0, V(4));
                }
                else
                {
                    item.Font = Typography.Scaled(Typography.Overline, textScale);
                    item.Size = new Size(inner, V(menuPanel.Controls.GetChildIndex(item) == 0 ? 22 : 36));
                    item.Padding = new Padding(S(16), 0, 0, V(6));
                }
            }
            menuPanel.ResumeLayout();
            menuPanel.SetBounds(pad, top, inner, Math.Max(100, room));
            menuPanel.AutoScroll = units * tall > room + 2;   // only on a very short window
        }

        // The sidebar is always dark. The screen area follows light or dark mode.
        private void ApplyColors()
        {
            sidebarPanel.BackColor = Ui.SidebarBackground;
            logoLabel.ForeColor = Tints.Carbon.White;

            BackColor = Ui.PageBackground;
            contentPanel.BackColor = Ui.PageBackground;
            screenPanel.BackColor = Ui.PageBackground;
            titleLabel.ForeColor = Ui.Colors.TextPrimary;
            crumbLabel.ForeColor = Ui.Colors.TextSecondary;
            subtitleLabel.ForeColor = Ui.Colors.TextSecondary;
            actionsPanel.BackColor = Ui.PageBackground;

            // The header follows the type scale: small breadcrumb, big page title, supporting line under it.
            crumbLabel.Font = Typography.Meta;
            subtitleLabel.Font = Typography.Support;
            titleLabel.Font = Typography.PageTitle;
            foreach (Control item in menuPanel.Controls) { item.BackColor = Ui.SidebarBackground; item.Invalidate(); }
            profileCard.Invalidate();
            themeToggle.Invalidate();
            backupRow.Invalidate();
            RepaintScreen();
        }

        // The bottom of the sidebar: the dark mode switch, then a card with who is signed in and which store this is.

        private void BuildProfile()
        {
            profileCard.UserName = Session.UserName ?? "Store owner";
            profileCard.Role = Session.Role ?? "Admin";
            themeToggle.Click += (sender, e) => ToggleDarkMode();

            // Backing up is the store owner's job, so only the Admin sees the button.
            // Signing out backs up for every role, because cashiers add most of the day's sales.
            backupRow.Visible = (Session.Role ?? "Admin") == "Admin";
            backupRow.Click += (sender, e) => BackUpNow();
            profileCard.SignOutClicked += (sender, e) => SignOut(exitApp: false);
            ShowBackupStatus();

            menuPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        // ---------- cloud backup (local → cloud) ----------

        // True after "Sign out", so the sign-in window opens again instead of the app closing.
        public bool SignedOut { get; private set; }
        private bool _leaving;

        private void ShowBackupStatus()
        {
            string code = Session.Store?.Code;
            if (code == null) return;
            BackupStatus status = CloudBackup.LoadStatus(code);
            BackupResult failure = status.LastFailure;
            bool failedLast = failure != null && (status.LastSuccess == null || failure.When > status.LastSuccess);

            // The line under "Back up now" is short so it fits the sidebar; the tooltip has the full details.
            backupRow.Failed = failedLast || !CloudBackup.IsSetUp;
            if (!CloudBackup.IsSetUp) backupRow.Status = "Not set up on this PC";
            else if (failedLast) backupRow.Status = "Failed · " + ShortReason(failure.Outcome);
            else if (status.LastSuccess != null) backupRow.Status = "Last backup · " + WhenShort(status.LastSuccess.Value);
            else backupRow.Status = "Not backed up yet";

            string details = status.LastSuccess != null
                ? "Last backup: " + status.LastSuccess.Value.ToString("MMM d, yyyy, h:mm tt") + " (" + status.Rows.ToString("N0") + " rows)"
                : "No backup yet";
            if (failedLast) details += "\nLast try " + failure.When.ToString("MMM d, h:mm tt") + ": " + failure.Message;
            _backupTip.SetToolTip(backupRow, details);
            backupRow.Invalidate();
        }

        private readonly ToolTip _backupTip = new ToolTip { AutoPopDelay = 15000 };

        // Today shows the time ("1:05 PM"), yesterday says "yesterday", older ones show the date ("Nov 16").
        private static string WhenShort(DateTime when)
        {
            if (when.Date == DateTime.Today) return when.ToString("h:mm tt");
            if (when.Date == DateTime.Today.AddDays(-1)) return "yesterday";
            return when.ToString("MMM d");
        }

        private static string ShortReason(BackupOutcome outcome)
        {
            return outcome switch
            {
                BackupOutcome.Offline => "no internet",
                BackupOutcome.WrongPassword => "login refused",
                BackupOutcome.NotUpToDate => "update cloud",
                BackupOutcome.NotSetUp => "not set up",
                _ => "error"
            };
        }

        // Runs the backup behind a "Backing up…" pop-up and returns what happened.
        private BackupResult RunBackup(string title, string description)
        {
            StoreInfo store = Session.Store;
            backupRow.Busy = true;
            backupRow.Invalidate();
            using BackupDialog dialog = new BackupDialog(title, description, () => CloudBackup.RunAsync(store));
            dialog.ShowModal(this);
            backupRow.Busy = false;
            ShowBackupStatus();

            BackupResult result = dialog.Result;
            if (result != null)
                Log(result.Outcome == BackupOutcome.Done
                    ? "Backed up to the cloud · " + (result.Synced == 0 ? "nothing new" : result.Synced.ToString("N0") + " records synced")
                    : "Backup failed · " + ShortReason(result.Outcome));
            if (_activeScreen is ActivityScreen activity) activity.Reload();   // the lines turn to "Synced to cloud"
            return result;
        }

        // Writes a line to the Activity log. A log line must never stop signing out, so a failure is ignored.
        private static void Log(string action)
        {
            try { StoreDb.LogEvent(action); }
            catch (Exception) { }
        }

        // The sidebar button and the Activity log page both use this.
        public void BackUpNow()
        {
            if (Session.Store == null || backupRow.Busy) return;
            BackupResult result = RunBackup("Backing up to the cloud", "Copying " + _storeName + "'s data to its cloud database.");
            bool done = result != null && result.Outcome == BackupOutcome.Done;
            Notice.Show(this, done ? "Backed up" : "Not backed up", result?.Message ?? "The backup did not run.",
                done ? NoticeKind.Success : NoticeKind.Warning);
        }

        // Signing out (or closing the window) backs up first, then opens the sign-in window again (or closes xpera).
        // Without internet it still signs out: the data is safe on this PC and the next sign-out tries again.
        private void SignOut(bool exitApp)
        {
            if (_leaving) return;
            _leaving = true;
            if (Session.Store != null && CloudBackup.ShouldRunOnSignOut(Session.Store.Code))
            {
                BackupResult result = RunBackup("Backing up before you sign out", "Copying " + _storeName + "'s data to its cloud database.");
                if (result != null && result.Outcome != BackupOutcome.Done)
                    Notice.Show(this, "Signed out without a backup", result.Message + " Your data is still safe on this PC.", NoticeKind.Warning);
            }
            if (Session.Store != null) Log("Signed out");
            SignedOut = !exitApp;
            Session.Clear();
            Close();
        }

        // The window's × counts as signing out too, so the day's work is backed up before xpera closes.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_leaving && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                BeginInvoke(new Action(() => SignOut(exitApp: true)));
                return;
            }
            base.OnFormClosing(e);
        }

        // Shows which store is signed in, at the bottom of the sidebar and in the breadcrumb.
        public void ShowStore(string name, string plan)
        {
            profileCard.Store = name;
            profileCard.Plan = plan;
            profileCard.Invalidate();
            _storeName = name.Split('·')[0].Trim();
        }

        private void ToggleDarkMode()
        {
            Ui.SetDarkMode(!Ui.IsDarkMode);
            ApplyColors();
        }

        private string _lastGroup;

        private void AddScreen(string group, string name, string roles, string subtitle, Func<UserControl> make)
        {
            string role = Session.Role ?? "Admin";
            if (!roles.Split(' ').Contains(role)) return;
            if (group != _lastGroup) { AddGroup(group); _lastGroup = group; }
            AddMenuItem(group, name, subtitle, make());
        }

        // A small gray heading in the menu, like "SELL".
        private void AddGroup(string name)
        {
            Label heading = new Label();
            heading.Text = name.ToUpper();
            heading.Font = Ui.UiFont(7.5F, true);
            heading.ForeColor = Tints.Carbon.Gray;
            heading.Size = new Size(216, menuPanel.Controls.Count == 0 ? 22 : 40);
            heading.TextAlign = ContentAlignment.BottomLeft;
            heading.Padding = new Padding(16, 0, 0, 6);
            heading.Margin = new Padding(0);
            menuPanel.Controls.Add(heading);
        }

        private void AddMenuItem(string group, string name, string subtitle, UserControl screen)
        {
            Xpera.Controls.MenuButton button = new Xpera.Controls.MenuButton();
            button.Icon = name;
            button.Font = Ui.UiFont(10.5F, false);
            button.Text = name;
            button.Width = 216;
            button.Height = 44;
            button.Margin = new Padding(0, 0, 0, 4);
            button.Click += (sender, e) => OpenScreen(button, group, name, subtitle, screen);
            menuPanel.Controls.Add(button);
        }

        private void OpenScreen(Xpera.Controls.MenuButton button, string group, string name, string subtitle, UserControl screen)
        {
            if (_activeButton != null) _activeButton.Active = false;
            button.Active = true;
            _activeButton = button;

            crumbLabel.Text = _storeName + "   ›   " + group + "   ›   " + name;
            titleLabel.Text = name;
            subtitleLabel.Text = subtitle;
            screen.Dock = DockStyle.Top;
            screenPanel.Controls.Clear();
            screenPanel.Controls.Add(screen);
            _activeScreen = screen;
            screenPanel.AutoScrollPosition = Point.Empty;
            FitScreen();

            // Each screen can put its main buttons in the header, like + Add or Checkout / Sales history.
            actionsPanel.Controls.Clear();
            if (screen is IHeaderActions header) actionsPanel.Controls.AddRange(header.HeaderActions());
            PlaceActions();

            // Screens stay in memory, so repaint one in case the mode changed while it was closed.
            RepaintScreen();
        }

        // The screen is as tall as the window, or as tall as its content when that is more, which makes
        // the screen area scroll. Only one scrollbar: tables grow instead of scrolling inside.
        private void FitScreen()
        {
            if (_activeScreen == null) return;
            int need = _activeScreen is IContentHeight sized ? sized.ContentHeight
                : _activeScreen.Controls.Cast<Control>().Where(c => c.Visible && c.Dock == DockStyle.Top).Sum(c => c.Height);
            int height = Math.Max(screenPanel.ClientSize.Height, need);
            if (_activeScreen.Height != height) _activeScreen.Height = height;
        }

        // The mouse wheel scrolls the screen area wherever the pointer is on it, even over a table,
        // because the tables have no scrollbar of their own to take the wheel.
        private class WheelScroll : IMessageFilter
        {
            private readonly MainForm _form;
            public WheelScroll(MainForm form) { _form = form; }

            public bool PreFilterMessage(ref Message m)
            {
                const int WM_MOUSEWHEEL = 0x020A;
                Panel area = _form.screenPanel;
                if (m.Msg != WM_MOUSEWHEEL || Form.ActiveForm != _form || !area.VerticalScroll.Visible) return false;
                Point pointer = area.PointToClient(Cursor.Position);
                if (!area.ClientRectangle.Contains(pointer)) return false;
                int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                int y = -area.AutoScrollPosition.Y - delta * Sizes.Px(area, 48) / 120;
                area.AutoScrollPosition = new Point(0, Math.Max(0, y));
                return true;
            }
        }

        private void PlaceActions()
        {
            int x = contentPanel.ClientSize.Width - contentPanel.Padding.Right - actionsPanel.Width;
            actionsPanel.Location = new Point(Math.Max(0, x), titleLabel.Top + 2);
            actionsPanel.BringToFront();
        }

        private void RepaintScreen()
        {
            Ui.ApplyTheme(screenPanel);
            Ui.ApplyTheme(actionsPanel);
            if (_activeScreen is IThemeAware screen) screen.ThemeChanged();
        }
    }

    // A screen whose main buttons go in the header, on the right of the title.
    public interface IHeaderActions
    {
        Control[] HeaderActions();
    }

    // A screen that knows how tall its content is, when that isn't just its top-docked parts added up.
    public interface IContentHeight
    {
        int ContentHeight { get; }
    }

    // A screen with parts that ApplyTheme cannot color by itself, like the Cash / Credit switch.
    public interface IThemeAware
    {
        void ThemeChanged();
    }
}

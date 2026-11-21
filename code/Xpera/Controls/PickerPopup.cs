using System.Runtime.InteropServices;

namespace Xpera.Controls
{
    // A pop-up list you can search, used to pick the customer and to add a product at checkout.
    // A store with 40 customers is too many for a plain menu, so you type a few letters instead,
    // then pick with the mouse or with the arrow keys and Enter. Esc or clicking away closes it.
    public class PickerPopup : Form
    {
        public class Item
        {
            public string Title = "";
            public string Detail = "";     // gray second line, like what they usually buy
            public string Side = "";       // right side, like a price or what they owe
            public string Badge = "";      // initials in the circle on the left
            public bool Enabled = true;
            public bool Current;           // the one already picked gets a red mark
            public string Icon = "";       // a menu icon instead of initials, like the box for products
            public Action OnPick;
        }

        private readonly List<Item> _items = new List<Item>();
        private readonly SearchBox _search = new SearchBox();
        private readonly ListBox _list = new ListBox();
        private readonly Label _hint = new Label();
        private const int RowHeight = Sizes.PickerRow;

        public PickerPopup(string placeholder, bool searchable = true)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            KeyPreview = true;
            BackColor = Ui.Colors.BackgroundPrimary;
            Padding = new Padding(10);

            _search.PlaceholderText = placeholder;
            _search.Dock = DockStyle.Top;
            _search.TextChanged += (sender, e) => Fill();

            _list.Dock = DockStyle.Fill;
            _list.BorderStyle = BorderStyle.None;
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.ItemHeight = RowHeight;
            _list.IntegralHeight = false;
            _list.BackColor = Ui.Colors.BackgroundPrimary;
            _list.DrawItem += DrawRow;
            _list.MouseMove += (sender, e) =>
            {
                int at = _list.IndexFromPoint(e.Location);
                if (at >= 0 && at != _list.SelectedIndex) _list.SelectedIndex = at;
            };
            _list.MouseClick += (sender, e) => Pick();

            _hint.Dock = DockStyle.Bottom;
            _hint.Height = 26;
            _hint.TextAlign = ContentAlignment.BottomLeft;
            _hint.Font = Ui.UiFont(8F, false);
            _hint.ForeColor = Ui.Colors.TextSecondary;
            _hint.Text = "↑ ↓ to move  ·  Enter to pick  ·  Esc to close";
            _hint.Tag = "muted";

            Panel gap = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = BackColor };
            Controls.Add(_list);
            Controls.Add(gap);
            Controls.Add(_search);
            // A short list (like a role) needs no search box.
            _search.Visible = searchable;
            gap.Visible = searchable;
            Controls.Add(_hint);
            Ui.ApplyTheme(this);
            Deactivate += (sender, e) => { if (!IsDisposed) BeginInvoke(new Action(() => { if (!IsDisposed) Close(); })); };
        }

        public Item AddItem(string title, string detail, string side, string badge, bool enabled, bool current, Action onPick)
        {
            Item item = new Item { Title = title, Detail = detail, Side = side, Badge = badge, Enabled = enabled, Current = current, OnPick = onPick };
            _items.Add(item);
            return item;
        }

        // Opens under the control, or above it when the screen has no room below.
        public void ShowBy(Control anchor, int width)
        {
            Fill();
            int rows = Math.Min(7, Math.Max(3, _items.Count));
            int search = _search.Visible ? _search.Height + 8 : 0;
            rows = _search.Visible ? rows : Math.Min(7, _items.Count);
            Size = new Size(Math.Max(width, 300), Padding.Vertical + search + rows * RowHeight + _hint.Height);

            Rectangle screen = Screen.FromControl(anchor).WorkingArea;
            Point below = anchor.PointToScreen(new Point(0, anchor.Height + 6));
            int y = below.Y + Height <= screen.Bottom ? below.Y : anchor.PointToScreen(Point.Empty).Y - Height - 6;
            int x = Math.Min(below.X, screen.Right - Width - 8);
            Location = new Point(Math.Max(screen.Left + 8, x), Math.Max(screen.Top + 8, y));
            Show(anchor.FindForm());
            if (_search.Visible) _search.FocusInput(); else _list.Focus();
        }

        private void Fill()
        {
            string query = _search.Text.Trim();
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (Item item in _items)
            {
                if (query == "" || item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || item.Detail.Contains(query, StringComparison.OrdinalIgnoreCase))
                    _list.Items.Add(item);
            }
            _list.EndUpdate();
            // Start on the first one that can be picked, so Enter works right away.
            for (int i = 0; i < _list.Items.Count; i++)
            {
                if (((Item)_list.Items[i]).Enabled) { _list.SelectedIndex = i; break; }
            }
            _list.Invalidate();
        }

        private void Pick()
        {
            if (!(_list.SelectedItem is Item item) || !item.Enabled) return;
            Hide();
            item.OnPick?.Invoke();
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            else if (e.KeyCode == Keys.Enter) Pick();
            else if (e.KeyCode == Keys.Down && _list.SelectedIndex < _list.Items.Count - 1) _list.SelectedIndex++;
            else if (e.KeyCode == Keys.Up && _list.SelectedIndex > 0) _list.SelectedIndex--;
            else { base.OnKeyDown(e); return; }
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void DrawRow(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            Item item = (Item)_list.Items[e.Index];
            Graphics g = e.Graphics;
            bool selected = (e.State & DrawItemState.Selected) != 0 && item.Enabled;

            using (SolidBrush back = new SolidBrush(Ui.Colors.BackgroundPrimary)) g.FillRectangle(back, e.Bounds);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle row = new Rectangle(e.Bounds.X, e.Bounds.Y + 2, e.Bounds.Width - 1, e.Bounds.Height - 4);
            if (selected)
            {
                using (var path = Ui.Rounded(row, 8))
                using (SolidBrush hover = new SolidBrush(Ui.Colors.BackgroundSecondary)) g.FillPath(hover, path);
            }

            // The circle on the left: Carbon on light cards, Bone on dark ones, like the customer avatar.
            bool hasCircle = item.Badge != "" || item.Icon != "";
            Rectangle circle = hasCircle ? new Rectangle(row.X + 8, row.Y + (row.Height - 32) / 2, 32, 32) : new Rectangle(row.X, row.Y, 0, 0);
            Color fill = item.Enabled ? (Ui.IsDarkMode ? Ui.Bone : Ui.Colors.Secondary) : Ui.Colors.BorderPrimary;
            Color ink = item.Enabled ? (Ui.IsDarkMode ? Ui.Colors.Secondary : Ui.Bone) : Ui.Colors.TextSecondary;
            if (hasCircle) using (SolidBrush dot = new SolidBrush(fill)) g.FillEllipse(dot, circle);
            if (!hasCircle) { }
            else if (item.Icon != "") MenuButton.DrawIcon(g, item.Icon, new Rectangle(circle.X + 7, circle.Y + 7, 18, 18), ink);
            else TextRenderer.DrawText(g, item.Badge, Ui.UiFont(8.5F, true), circle, ink, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            Color title = item.Enabled ? Ui.Colors.TextPrimary : Ui.Colors.TextSecondary;
            Color red = Ui.IsDarkMode ? Theme.Tints.SignalRed.SoftRose : Theme.Shades.SignalRed.Crimson;
            int textX = circle.Right + 12;
            int sideWidth = 120;
            Rectangle top = new Rectangle(textX, row.Y + 4, row.Right - textX - sideWidth, 20);
            Rectangle bottom = new Rectangle(textX, row.Y + 23, row.Right - textX - sideWidth, 18);
            if (item.Detail == "") top = new Rectangle(textX, row.Y, top.Width, row.Height);
            TextRenderer.DrawText(g, item.Title, item.Detail != "" || item.Current ? Typography.BodyStrong : Typography.Body, top, title,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | (item.Detail == "" ? TextFormatFlags.VerticalCenter : 0));
            TextRenderer.DrawText(g, item.Detail, Typography.Meta, bottom, Ui.Colors.TextSecondary, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            string side = item.Current ? "Picked" : item.Side;
            Rectangle right = new Rectangle(row.Right - sideWidth - 10, row.Y, sideWidth, row.Height);
            TextRenderer.DrawText(g, side, item.Current ? Typography.Label : Typography.Get(10F, Weight.Medium), right, item.Current || !item.Enabled ? red : Ui.Colors.TextPrimary,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        // A thin border, rounded corners on Windows 11, and the small shadow Windows gives pop-ups.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return p;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int round = 2; // DWMWCP_ROUND
            try { DwmSetWindowAttribute(Handle, 33, ref round, sizeof(int)); } catch (DllNotFoundException) { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen border = new Pen(Ui.Colors.BorderPrimary)) e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    }
}

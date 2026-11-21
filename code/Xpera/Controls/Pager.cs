namespace Xpera.Controls
{
    // The bar under a list: "1–8 of 210" on the left, and on the right a short row of page buttons:
    // ‹ Previous  1  …  4  5  6  …  27  Next ›
    // It always shows the first page, the last page, and the pages next to the current one, with "…" for
    // the rest, so a list with 27 pages never shows 27 buttons. Previous and Next are grayed out at the ends.
    public class Pager : Panel
    {
        private readonly Label _range = new Label();
        private readonly FlowLayoutPanel _buttons = new FlowLayoutPanel();
        private int _page;
        private int _pages = 1;

        // Runs with the new page number when a page button is clicked.
        public event Action<int> PageChanged;

        public Pager()
        {
            Height = 40;
            _range.Dock = DockStyle.Left;
            _range.AutoSize = false;
            _range.Width = 200;
            _range.TextAlign = ContentAlignment.MiddleLeft;
            _range.Tag = "muted";
            _range.Font = Typography.Meta;

            _buttons.Dock = DockStyle.Right;
            _buttons.AutoSize = true;
            _buttons.WrapContents = false;

            Controls.Add(_buttons);
            Controls.Add(_range);
        }

        public void Show(int page, int pages, int from, int to, int total)
        {
            _page = page;
            _pages = pages;
            _range.Text = total == 0 ? "" : from + "–" + to + " of " + total;
            Restyle();
        }

        // The page numbers to show (0-based), with -1 where a "…" goes.
        public static List<int> Window(int page, int pages)
        {
            List<int> shown = new List<int>();
            for (int i = 0; i < pages; i++)
            {
                bool keep = i == 0 || i == pages - 1 || Math.Abs(i - page) <= 1
                    || (page <= 2 && i <= 3) || (page >= pages - 3 && i >= pages - 4);   // near an end, show 4 in a row
                if (keep) shown.Add(i);
                else if (shown.Count > 0 && shown[shown.Count - 1] != -1) shown.Add(-1);
            }
            return shown;
        }

        // Rebuilds the buttons with the current colors. Also called when dark mode changes.
        public void Restyle()
        {
            _range.ForeColor = Ui.Colors.TextSecondary;
            _buttons.SuspendLayout();
            _buttons.Controls.Clear();
            _buttons.Padding = new Padding(0, Sizes.Px(this, 5), 0, 0);
            if (_pages > 1)
            {
                AddButton("‹  Previous", _page - 1, _page > 0, false, true);
                foreach (int i in Window(_page, _pages))
                {
                    if (i < 0) AddDots();
                    else AddButton((i + 1).ToString(), i, true, i == _page, false);
                }
                AddButton("Next  ›", _page + 1, _page < _pages - 1, false, true);
            }
            _buttons.ResumeLayout();
        }

        private void AddDots()
        {
            Label dots = new Label { Text = "…", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter, Tag = "muted" };
            dots.Font = Typography.Meta;
            dots.ForeColor = Ui.Colors.TextSecondary;
            dots.Size = new Size(Sizes.Px(this, 22), Sizes.Px(this, 30));
            dots.Margin = new Padding(Sizes.Px(this, 2), 0, 0, 0);
            _buttons.Controls.Add(dots);
        }

        private void AddButton(string text, int goTo, bool enabled, bool current, bool wide)
        {
            RoundButton button = new RoundButton();
            button.Radius = 8;
            button.Text = text;
            button.Font = current ? Typography.Label : Typography.Get(9F, Weight.Medium);
            int width = wide ? TextRenderer.MeasureText(text, button.Font).Width + Sizes.Px(this, 22) : Sizes.Px(this, 32);
            button.Size = new Size(width, Sizes.Px(this, 30));
            button.Margin = new Padding(Sizes.Px(this, 4), 0, 0, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.Enabled = enabled;
            button.Cursor = enabled ? Cursors.Hand : Cursors.Default;
            button.Tag = "keep";
            // Previous / Next have no box, so the page numbers stay the focus.
            button.FlatAppearance.BorderColor = current ? Ui.Colors.Secondary : wide ? Ui.Colors.BackgroundPrimary : Ui.Colors.BorderPrimary;
            button.BackColor = current ? Ui.Colors.Secondary : Ui.Colors.BackgroundPrimary;
            button.ForeColor = current ? Ui.Bone : Ui.Colors.TextPrimary;
            if (current && Ui.IsDarkMode) { button.BackColor = Ui.Bone; button.ForeColor = Ui.Colors.Secondary; }
            button.AccessibleName = wide ? text.Trim('‹', '›', ' ') : "Page " + text;
            if (enabled && !current) button.Click += (sender, e) => PageChanged?.Invoke(goTo);
            _buttons.Controls.Add(button);
        }
    }
}

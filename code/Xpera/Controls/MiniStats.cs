namespace Xpera.Controls
{
    // A few small label-over-number pairs in a row, like SPENT ₱45,200 | LAST VISIT Oct 3 on the customer card.
    public class MiniStats : Control
    {
        private List<StatStrip.Tile> _items = new List<StatStrip.Tile>();

        public MiniStats()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(300, 48);
        }

        public void SetItems(params StatStrip.Tile[] items)
        {
            _items = items.ToList();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.Colors.BackgroundPrimary);
            // In the Designer there is no customer yet, so it shows sample pairs to show where they go.
            List<StatStrip.Tile> items = _items.Count > 0 || !DesignMode ? _items
                : new[] { "Spent", "Last visit", "Balance" }.Select(n => StatStrip.Make(n, "—", "")).ToList();
            if (items.Count == 0) return;
            float d = DeviceDpi / 96f;
            int P(float v) => (int)Math.Round(v * d);
            int width = Width / items.Count;
            Color red = Ui.IsDarkMode ? Theme.Tints.SignalRed.SoftRose : Theme.Shades.SignalRed.Crimson;
            for (int i = 0; i < items.Count; i++)
            {
                int x = i * width;
                // A thin divider between pairs, so they read as separate facts.
                if (i > 0)
                {
                    using Pen line = new Pen(Ui.Colors.BorderPrimary);
                    g.DrawLine(line, x, P(6), x, Height - P(6));
                }
                Rectangle top = new Rectangle(x + P(14), P(3), width - P(18), P(18));
                Rectangle bottom = new Rectangle(x + P(14), P(23), width - P(18), P(24));
                TextRenderer.DrawText(g, items[i].Label.ToUpper(), Ui.UiFont(7.5F, true), top, Ui.Colors.TextSecondary, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, items[i].Value, Typography.Money, bottom, items[i].Alert ? red : Ui.Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}

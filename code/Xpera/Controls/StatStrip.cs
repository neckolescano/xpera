using System.Drawing.Drawing2D;
using Xpera.Theme;

namespace Xpera.Controls
{
    // A row of small number tiles above a list, like "₱120,000 owed · 3 overdue".
    // They answer the first question someone has on each screen before they read a single row.
    // A tile marked Alert shows its number in red, so the one thing that needs action stands out.
    public class StatStrip : Control
    {
        public class Tile
        {
            public string Label = "";
            public string Value = "";
            public string Note = "";
            public bool Alert;
        }

        private List<Tile> _tiles = new List<Tile>();
        private const int Gap = 14;

        public StatStrip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Dock = DockStyle.Top;
            Height = TileHeight + Gap;   // the tiles, then a gap before the card below (at 100% display scaling)
        }

        private float Dpi { get { return DeviceDpi / 96f; } }

        // The height is set here from the display scaling, not stretched by the screen's own scaling.
        private const int TileHeight = 96;
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Height = (int)((TileHeight + Gap) * Dpi); }
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified) { base.ScaleControl(factor, specified & ~BoundsSpecified.Height); }

        public void SetTiles(params Tile[] tiles)
        {
            _tiles = tiles.ToList();
            Invalidate();
        }

        public static Tile Make(string label, string value, string note, bool alert = false)
        {
            return new Tile { Label = label, Value = value, Note = note, Alert = alert };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.PageBackground);
            // In the Designer there is no data yet, so it shows four sample tiles to show where they go.
            List<Tile> tiles = _tiles.Count > 0 || !DesignMode ? _tiles
                : Enumerable.Range(1, 4).Select(n => Make("Tile " + n, "0", "filled in when the page loads")).ToList();
            if (tiles.Count == 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float d = Dpi;
            int P(float v) => (int)Math.Round(v * d);
            int gap = P(Gap);
            int width = (Width - gap * (tiles.Count - 1)) / tiles.Count;
            Color red = Ui.IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson;
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                Rectangle box = new Rectangle(i * (width + gap), 0, width - 1, Height - gap - 1);
                using (GraphicsPath path = Ui.Rounded(box, P(14)))
                using (SolidBrush fill = new SolidBrush(Ui.Colors.BackgroundPrimary))
                using (Pen border = new Pen(Ui.Colors.BorderSecondary))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(border, path);
                }
                // A short red tick on the alert tile, the same mark as the open menu item.
                if (tile.Alert)
                {
                    using (GraphicsPath bar = Ui.Rounded(new Rectangle(box.X, box.Y + P(16), P(3), P(26)), 1))
                    using (SolidBrush brush = new SolidBrush(Ui.Colors.Primary)) g.FillPath(brush, bar);
                }

                int x = box.X + P(18), w = box.Width - P(36);
                TextRenderer.DrawText(g, tile.Label.ToUpper(), Ui.UiFont(7.5F, true), new Rectangle(x, box.Y + P(13), w, P(17)), Ui.Colors.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, tile.Value, Typography.Number, new Rectangle(x - P(2), box.Y + P(30), w, P(32)), tile.Alert ? red : Ui.Colors.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, tile.Note, Ui.UiFont(8.5F, false), new Rectangle(x, box.Y + P(63), w, P(19)), Ui.Colors.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}

using System.ComponentModel;
using System.Drawing.Drawing2D;
using Xpera.Theme;

namespace Xpera.Controls
{
    // One item in the sidebar: a small line icon, the screen name, and a red bar when it is open.
    // The open item is lifted one shade instead of filled red, so the red stays for the one thing
    // that matters on each screen (like Save sale), and the sidebar reads calm.
    public class MenuButton : Button
    {
        private bool _hover;
        private bool _active;

        [DefaultValue("")]
        public string Icon { get; set; } = "";

        // How much bigger than normal it is drawn (1 = 44 high). Set by the sidebar on big screens.
        [DefaultValue(1f)]
        public float SizeScale { get; set; } = 1f;

        [DefaultValue(false)]
        public bool Active
        {
            get { return _active; }
            set { _active = value; Invalidate(); }
        }

        public MenuButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Ui.SidebarBackground);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            if (_active || _hover)
            {
                Color fill = _active ? Tints.Carbon.DarkCharcoal : Tones.Carbon.VeryDarkGray;
                using (GraphicsPath path = Ui.Rounded(box, 8))
                using (SolidBrush brush = new SolidBrush(fill)) g.FillPath(brush, path);
            }
            if (_active)
            {
                int inset = (int)(11 * SizeScale);
                using (GraphicsPath bar = Ui.Rounded(new Rectangle(0, inset, 4, Height - inset * 2 - 1), 2))
                using (SolidBrush red = new SolidBrush(Ui.Colors.Primary)) g.FillPath(red, bar);
            }

            Color ink = _active ? Tints.Carbon.White : _hover ? Tints.Carbon.White : Tints.Carbon.LightGray;
            Color iconInk = _active ? Ui.Colors.Primary : ink;
            // The icon is drawn at its normal 18 px and scaled up, so its lines stay in proportion.
            float iconSize = 18 * SizeScale;
            g.TranslateTransform(18 * SizeScale, (Height - iconSize) / 2);
            g.ScaleTransform(SizeScale, SizeScale);
            DrawIcon(g, Icon, new Rectangle(0, 0, 18, 18), iconInk);
            g.ResetTransform();

            int textX = (int)(52 * SizeScale);
            Rectangle text = new Rectangle(textX, 0, Width - textX - 6, Height);
            TextRenderer.DrawText(g, Text, Font, text, ink, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // Simple line icons, drawn with the same pen so they all match. Each one is a sketch of what the screen holds.
        public static void DrawIcon(Graphics g, string icon, Rectangle r, Color color)
        {
            using Pen pen = new Pen(color, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            int x = r.X, y = r.Y, w = r.Width, h = r.Height;
            switch (icon)
            {
                case "Sales":       // a receipt with a torn bottom
                    g.DrawLines(pen, new[] { new Point(x + 3, y + 1), new Point(x + 15, y + 1), new Point(x + 15, y + 17), new Point(x + 12, y + 15),
                                             new Point(x + 9, y + 17), new Point(x + 6, y + 15), new Point(x + 3, y + 17), new Point(x + 3, y + 1) });
                    g.DrawLine(pen, x + 6, y + 6, x + 12, y + 6);
                    g.DrawLine(pen, x + 6, y + 10, x + 10, y + 10);
                    break;
                case "Payments":    // a wallet
                    using (GraphicsPath wallet = Ui.Rounded(new Rectangle(x + 1, y + 4, w - 2, h - 7), 3)) g.DrawPath(pen, wallet);
                    g.DrawLine(pen, x + 3, y + 4, x + 13, y + 1);
                    g.DrawEllipse(pen, x + 11, y + 9, 2, 2);
                    break;
                case "Products":    // a box
                    g.DrawPolygon(pen, new[] { new Point(x + 9, y + 1), new Point(x + 17, y + 5), new Point(x + 17, y + 13), new Point(x + 9, y + 17), new Point(x + 1, y + 13), new Point(x + 1, y + 5) });
                    g.DrawLines(pen, new[] { new Point(x + 1, y + 5), new Point(x + 9, y + 9), new Point(x + 17, y + 5) });
                    g.DrawLine(pen, x + 9, y + 9, x + 9, y + 17);
                    break;
                case "Customers":   // two people
                    g.DrawEllipse(pen, x + 3, y + 2, 6, 6);
                    g.DrawArc(pen, x, y + 10, 12, 12, 180, 180);
                    g.DrawArc(pen, x + 10, y + 3, 5, 5, 270, 270);
                    g.DrawArc(pen, x + 10, y + 11, 8, 10, 250, 110);
                    break;
                case "Inquiries":   // a speech bubble with a question mark
                    using (GraphicsPath bubble = Ui.Rounded(new Rectangle(x + 1, y + 1, w - 2, h - 6), 4)) g.DrawPath(pen, bubble);
                    g.DrawArc(pen, x + 6, y + 3, 6, 5, 180, 230);
                    g.DrawLine(pen, x + 9, y + 9, x + 9, y + 9.5f);
                    break;
                case "Complaints":  // a warning triangle
                    g.DrawPolygon(pen, new[] { new Point(x + 9, y + 1), new Point(x + 17, y + 16), new Point(x + 1, y + 16) });
                    g.DrawLine(pen, x + 9, y + 6, x + 9, y + 10);
                    g.DrawLine(pen, x + 9, y + 13, x + 9, y + 13.5f);
                    break;
                case "Feedback":    // a star
                    PointF[] star = new PointF[10];
                    for (int i = 0; i < 10; i++)
                    {
                        double angle = -Math.PI / 2 + i * Math.PI / 5;
                        float radius = i % 2 == 0 ? 8.5f : 3.8f;
                        star[i] = new PointF(x + 9 + (float)(Math.Cos(angle) * radius), y + 9.5f + (float)(Math.Sin(angle) * radius));
                    }
                    g.DrawPolygon(pen, star);
                    break;
                case "Users":       // a person with a key-card
                    g.DrawEllipse(pen, x + 5, y + 1, 8, 8);
                    g.DrawArc(pen, x + 1, y + 11, 16, 14, 180, 180);
                    break;
                case "Activity log": // a clock with an arrow going back around it (history)
                    g.DrawArc(pen, x + 1, y + 1, 16, 16, 200, 300);
                    g.DrawLines(pen, new[] { new Point(x + 1, y + 3), new Point(x + 2, y + 7), new Point(x + 6, y + 6) });
                    g.DrawLines(pen, new[] { new Point(x + 9, y + 5), new Point(x + 9, y + 9), new Point(x + 12, y + 11) });
                    break;
                default:
                    g.DrawEllipse(pen, x + 4, y + 4, w - 8, h - 8);
                    break;
            }
        }
    }
}

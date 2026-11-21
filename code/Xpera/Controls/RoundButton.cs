using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A button with rounded corners, a soft hover, and an optional border or dashed border.
    // It is still a normal Button, so Click and the Designer work the same way.
    public class RoundButton : Button
    {
        private bool _hover;
        private bool _down;

        [DefaultValue(10)]
        public int Radius { get; set; } = 10;

        // A dashed border, used for "+ Add item" so it reads as a place to add something.
        [DefaultValue(false)]
        public bool Dashed { get; set; }

        public RoundButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        // The corners are painted in the color behind the button, so redraw when that color changes (light/dark switch).
        protected override void OnParentBackColorChanged(EventArgs e) { base.OnParentBackColorChanged(e); Invalidate(); }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _down = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _down = false; Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Hover and press shift the color a little, toward black on light buttons and white on dark ones.
            Color fill = BackColor;
            if (Enabled && (_hover || _down)) fill = Shift(fill, _down ? 0.16f : 0.08f);

            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Ui.Rounded(box, Math.Min(Radius, Height / 2)))
            {
                using (SolidBrush brush = new SolidBrush(fill)) g.FillPath(brush, path);

                Color border = FlatAppearance.BorderColor;
                if (!border.IsEmpty && border != BackColor)
                {
                    using (Pen pen = new Pen(_hover && Enabled ? Shift(border, 0.2f) : border))
                    {
                        if (Dashed) pen.DashPattern = new float[] { 4, 3 };
                        g.DrawPath(pen, path);
                    }
                }
            }

            Color ink = Enabled ? ForeColor : Color.FromArgb(110, ForeColor);
            Rectangle textBox = new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height);
            TextRenderer.DrawText(g, Text, Font, textBox, ink, Flags(TextAlign) | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
            {
                using (Pen focus = new Pen(Ui.Colors.Primary, 1.5f))
                using (GraphicsPath ring = Ui.Rounded(new Rectangle(2, 2, Width - 5, Height - 5), Math.Max(2, Math.Min(Radius, Height / 2) - 2)))
                {
                    g.DrawPath(focus, ring);
                }
            }
        }

        private static Color Shift(Color color, float amount)
        {
            bool dark = color.GetBrightness() < 0.45f;
            int target = dark ? 255 : 0;
            return Color.FromArgb(color.A,
                (int)(color.R + (target - color.R) * amount),
                (int)(color.G + (target - color.G) * amount),
                (int)(color.B + (target - color.B) * amount));
        }

        private static TextFormatFlags Flags(ContentAlignment align)
        {
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
            if (align == ContentAlignment.MiddleLeft || align == ContentAlignment.TopLeft || align == ContentAlignment.BottomLeft) return flags | TextFormatFlags.Left;
            if (align == ContentAlignment.MiddleRight || align == ContentAlignment.TopRight || align == ContentAlignment.BottomRight) return flags | TextFormatFlags.Right;
            return flags | TextFormatFlags.HorizontalCenter;
        }
    }
}

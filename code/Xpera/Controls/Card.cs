using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A rounded box that groups one thing on a screen, like a list or the payment summary.
    // Inset cards sit inside another card and use the page color (Bone in light mode).
    public class Card : Panel
    {
        private bool _inset;

        [DefaultValue(false)]
        public bool Inset
        {
            get { return _inset; }
            set { _inset = value; ApplyColors(); }
        }

        [DefaultValue(14)]
        public int Radius { get; set; } = 14;

        public Card()
        {
            DoubleBuffered = true;
            Padding = new Padding(18, 16, 18, 16);
            ApplyColors();
        }

        public void ApplyColors()
        {
            BackColor = Inset ? Ui.PageBackground : Ui.Colors.BackgroundPrimary;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Paint the corners in the parent's color, then the rounded card on top.
            Color outside = Parent != null ? Parent.BackColor : Ui.PageBackground;
            using (SolidBrush corners = new SolidBrush(outside))
            {
                e.Graphics.FillRectangle(corners, ClientRectangle);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Ui.Rounded(box, Radius))
            using (SolidBrush fill = new SolidBrush(BackColor))
            using (Pen border = new Pen(Inset ? BackColor : Ui.Colors.BorderSecondary))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(border, path);
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }
    }
}

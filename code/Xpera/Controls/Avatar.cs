using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A Carbon circle with the customer's initials, like "MR" for Marco Reyes.
    public class Avatar : Control
    {
        private string _initials = "";

        [DefaultValue("")]
        public string Initials
        {
            get { return _initials; }
            set { _initials = value; Invalidate(); }
        }

        public Avatar()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
            Size = new Size(40, 40);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Carbon circle on light cards, Bone circle on dark cards, so it always stands out.
            // In the sidebar, which is always dark, it is always Bone.
            bool onDark = Ui.IsDarkMode || Equals(Tag, "sidebar");
            Color fill = onDark ? Ui.Bone : Ui.Colors.Secondary;
            Color ink = onDark ? Ui.Colors.Secondary : Ui.Bone;
            using (SolidBrush circle = new SolidBrush(fill))
            {
                e.Graphics.FillEllipse(circle, 0, 0, Width - 1, Height - 1);
            }
            TextRenderer.DrawText(e.Graphics, _initials, Ui.UiFont(Height >= 40 ? 10F : 8.5F, true), ClientRectangle, ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

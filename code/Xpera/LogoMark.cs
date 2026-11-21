using System.Drawing.Drawing2D;
using Xpera.Theme;

namespace Xpera
{
    // The spark X from the brand guide: three solid blades and one longer red blade.
    public class LogoMark : Control
    {
        private const float Diagonal = 0.7071f;

        public LogoMark()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // The mark is drawn on a 100 by 100 grid, then scaled to fit the control.
            g.ScaleTransform(Width / 100f, Height / 100f);
            g.TranslateTransform(-4.2f, 6.4f);

            using SolidBrush white = new SolidBrush(Tints.Carbon.White);
            using SolidBrush red = new SolidBrush(ColorTranslator.FromHtml("#F43F5E"));
            using SolidBrush softRed = new SolidBrush(Color.FromArgb(140, red.Color));

            g.FillPolygon(white, Blade(-1, -1, 40, 9.5f, 15));
            g.FillPolygon(white, Blade(-1, 1, 40, 9.5f, 15));
            g.FillPolygon(white, Blade(1, 1, 40, 9.5f, 15));

            // The red blade has two halves, which gives it the bevel.
            PointF[] blade = Blade(1, -1, 58, 8.5f, 13);
            g.FillPolygon(red, new[] { blade[0], blade[1], blade[2] });
            g.FillPolygon(softRed, new[] { blade[0], blade[2], blade[3] });
        }

        // A blade is a kite shape: center, one side, tip, other side.
        private static PointF[] Blade(int xDirection, int yDirection, float length, float width, float middle)
        {
            float dx = xDirection * Diagonal;
            float dy = yDirection * Diagonal;
            PointF center = new PointF(50, 50);
            PointF tip = new PointF(50 + dx * length, 50 + dy * length);
            float mx = 50 + dx * middle;
            float my = 50 + dy * middle;
            return new[]
            {
                center,
                new PointF(mx - dy * width, my + dx * width),
                tip,
                new PointF(mx + dy * width, my - dx * width)
            };
        }
    }
}

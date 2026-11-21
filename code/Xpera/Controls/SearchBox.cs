namespace Xpera.Controls
{
    // A rounded field with a magnifying glass, used above every list.
    public class SearchBox : InputBox
    {
        public SearchBox()
        {
            Height = 36;
            Padding = new Padding(34, 9, 12, 6);
        }

        protected override void PaintExtras(Graphics g)
        {
            // The magnifying glass: a circle and a short handle.
            using (Pen glass = new Pen(Ui.Colors.TextSecondary, 1.6f))
            {
                int cy = Height / 2;
                g.DrawEllipse(glass, 12, cy - 7, 11, 11);
                g.DrawLine(glass, 21, cy + 2, 25, cy + 6);
            }
        }
    }
}

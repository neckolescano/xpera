using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A rounded date box that matches the other fields. Clicking it opens a calendar to pick a day.
    // Value and ValueChanged work like a DateTimePicker.
    public class DateField : Control
    {
        private DateTime _value = DateTime.Today;
        private bool _open;

        public event EventHandler ValueChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Value
        {
            get { return _value; }
            set
            {
                if (_value == value.Date) return;
                _value = value.Date;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DateField()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Cursor = Cursors.Hand;
            Height = 40;
            TabStop = true;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
            OpenCalendar();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Down) OpenCalendar();
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        // The calendar opens under the box. Picking a day closes it and sets the date.
        private void OpenCalendar()
        {
            MonthCalendar calendar = new MonthCalendar();
            calendar.MaxSelectionCount = 1;
            calendar.SetDate(_value);
            calendar.MinDate = DateTime.Today;

            ToolStripControlHost host = new ToolStripControlHost(calendar);
            host.Margin = Padding.Empty;
            host.Padding = Padding.Empty;
            ToolStripDropDown dropDown = new ToolStripDropDown();
            dropDown.Padding = Padding.Empty;
            dropDown.Items.Add(host);

            calendar.DateSelected += (sender, e) => { Value = e.Start; dropDown.Close(); };
            dropDown.Closed += (sender, e) => { _open = false; Invalidate(); };
            _open = true;
            Invalidate();
            dropDown.Show(this, new Point(0, Height + 4));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.Colors.BackgroundPrimary);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Ui.Rounded(box, 10))
            using (SolidBrush fill = new SolidBrush(Ui.InputBackground))
            using (Pen border = new Pen(Focused || _open ? Ui.Colors.Primary : Ui.Colors.BorderPrimary))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            // The date as words, like "December 2, 2026".
            Rectangle text = new Rectangle(12, 0, Width - 48, Height);
            TextRenderer.DrawText(g, _value.ToString("MMMM d, yyyy"), Font, text, Ui.Colors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

            // A small calendar icon on the right: a box, two rings, and a line for the header.
            int x = Width - 30, y = Height / 2 - 8;
            using (Pen ink = new Pen(Ui.Colors.TextSecondary, 1.5f))
            {
                using (GraphicsPath cal = Ui.Rounded(new Rectangle(x, y + 2, 16, 14), 3)) g.DrawPath(ink, cal);
                g.DrawLine(ink, x, y + 7, x + 16, y + 7);
                g.DrawLine(ink, x + 4, y, x + 4, y + 4);
                g.DrawLine(ink, x + 12, y, x + 12, y + 4);
            }
        }
    }
}

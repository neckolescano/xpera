using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A dropdown in the same rounded style as the text fields. Clicking it (or pressing Enter, Space, or Alt+Down)
    // opens a list you can search, so a long list like 40 customers is quick to use.
    // Up and Down change the choice without opening the list.
    public class ChoiceBox : Control
    {
        public class Choice
        {
            public string Title = "";
            public string Detail = "";
            public string Side = "";
            public string Badge = "";
        }

        private readonly List<Choice> _choices = new List<Choice>();
        private int _selected = -1;
        private bool _hover;

        public event EventHandler SelectedIndexChanged;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool Searchable { get; set; } = true;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string SearchHint { get; set; } = "Search";

        public ChoiceBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Height = Sizes.Field;
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.ComboBox;
            Font = Ui.UiFont(10F, false);
        }

        public void AddChoice(string title, string detail = "", string side = "", string badge = "")
        {
            _choices.Add(new Choice { Title = title, Detail = detail, Side = side, Badge = badge });
            if (_selected < 0) SelectedIndex = 0;
        }

        public int Count { get { return _choices.Count; } }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return _selected; }
            set
            {
                if (value < -1 || value >= _choices.Count || value == _selected) return;
                _selected = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // The picked title, so the form reads a dropdown the same way as a text field.
        public override string Text
        {
            get { return _selected >= 0 ? _choices[_selected].Title : ""; }
            set { }
        }

        private void Open()
        {
            Focus();
            PickerPopup picker = new PickerPopup(SearchHint, Searchable);
            for (int i = 0; i < _choices.Count; i++)
            {
                int index = i;
                Choice c = _choices[i];
                picker.AddItem(c.Title, c.Detail, c.Side, c.Badge, true, i == _selected, () => SelectedIndex = index);
            }
            picker.ShowBy(this, Width);
        }

        protected override void OnClick(EventArgs e) { base.OnClick(e); Open(); }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Up || keyData == Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || (e.Alt && e.KeyCode == Keys.Down)) { Open(); e.Handled = true; }
            else if (e.KeyCode == Keys.Down && _selected < _choices.Count - 1) { SelectedIndex++; e.Handled = true; }
            else if (e.KeyCode == Keys.Up && _selected > 0) { SelectedIndex--; e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : Ui.Colors.BackgroundPrimary);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            Color border = Focused ? Ui.Colors.Primary : _hover ? Ui.Colors.TextSecondary : Ui.Colors.BorderPrimary;
            using (GraphicsPath path = Ui.Rounded(box, 10))
            using (SolidBrush fill = new SolidBrush(Ui.InputBackground))
            using (Pen pen = new Pen(border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }

            int x = 14;
            Choice c = _selected >= 0 ? _choices[_selected] : null;
            if (c != null && c.Badge != "")
            {
                Rectangle circle = new Rectangle(10, (Height - 26) / 2, 26, 26);
                using (SolidBrush dot = new SolidBrush(Ui.IsDarkMode ? Ui.Bone : Ui.Colors.Secondary)) g.FillEllipse(dot, circle);
                TextRenderer.DrawText(g, c.Badge, Ui.UiFont(7.5F, true), circle, Ui.IsDarkMode ? Ui.Colors.Secondary : Ui.Bone, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                x = 46;
            }

            int sideWidth = c != null && c.Side != "" ? TextRenderer.MeasureText(c.Side, Ui.UiFont(9F, true)).Width + 8 : 0;
            Rectangle text = new Rectangle(x, 0, Width - x - 40 - sideWidth, Height);
            TextRenderer.DrawText(g, c == null ? "Choose one" : c.Title, Font, text, c == null ? Ui.Colors.TextSecondary : Ui.Colors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if (sideWidth > 0)
            {
                TextRenderer.DrawText(g, c.Side, Ui.UiFont(9F, true), new Rectangle(Width - 36 - sideWidth, 0, sideWidth, Height), Ui.Colors.TextSecondary,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            // The chevron on the right.
            using Pen chevron = new Pen(Ui.Colors.TextSecondary, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            int cx = Width - 20, cy = Height / 2;
            g.DrawLines(chevron, new[] { new Point(cx - 5, cy - 2), new Point(cx, cy + 3), new Point(cx + 5, cy - 2) });
        }
    }
}

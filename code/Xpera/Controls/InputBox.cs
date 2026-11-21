using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Xpera.Controls
{
    // A rounded text field. The border turns red while you type in it.
    // Text, TextChanged, PlaceholderText, and password dots work like a normal TextBox.
    public class InputBox : Panel
    {
        protected readonly TextBox Input = new TextBox();

        [DefaultValue("")]
        public string PlaceholderText
        {
            get { return Input.PlaceholderText; }
            set { Input.PlaceholderText = value; }
        }

        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get { return Input.UseSystemPasswordChar; }
            set { Input.UseSystemPasswordChar = value; }
        }

        [DefaultValue(CharacterCasing.Normal)]
        public CharacterCasing CharacterCasing
        {
            get { return Input.CharacterCasing; }
            set { Input.CharacterCasing = value; }
        }

        // What may be typed. Wrong keys are ignored and pasted text is cleaned, so the field never holds
        // something that can't be saved (like a 12th digit in a mobile number). The full check runs on Save.
        [DefaultValue(InputKind.Any)]
        public InputKind Kind { get; set; } = InputKind.Any;

        // The most characters the field takes; matches the database column.
        // Counted after cleaning, so pasting "0956-563-4228" still keeps all 11 digits.
        [DefaultValue(32767)]
        public int MaxLength { get; set; } = 32767;

        // A sign drawn inside the field before the text, like ₱ for money.
        private string _prefix = "";

        [DefaultValue("")]
        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; Padding = new Padding(value == "" ? 12 : 32, Padding.Top, Padding.Right, Padding.Bottom); Invalidate(); }
        }

        public override string Text
        {
            get { return Input.Text; }
            set { Input.Text = value; }
        }

        public InputBox()
        {
            DoubleBuffered = true;
            Height = 40;
            Padding = new Padding(12, 10, 12, 6);
            Input.BorderStyle = BorderStyle.None;
            Input.Dock = DockStyle.Fill;
            Input.KeyPress += (sender, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                bool full = Input.TextLength - Input.SelectionLength >= MaxLength;
                if (!Allowed(e.KeyChar) || full) e.Handled = true;
            };
            Input.TextChanged += (sender, e) => { Clean(); OnTextChanged(e); };
            Input.Enter += (sender, e) => Invalidate();
            Input.Leave += (sender, e) => Invalidate();
            Controls.Add(Input);
            Click += (sender, e) => Input.Focus();
            ApplyColors();
        }

        private bool Allowed(char c)
        {
            switch (Kind)
            {
                case InputKind.Digits: return char.IsDigit(c);
                case InputKind.Money: return char.IsDigit(c) || (c == '.' && !Input.Text.Contains('.'));
                case InputKind.Name: return Rules.IsNameChar(c);
                case InputKind.Username: return Rules.IsUsernameChar(char.ToLowerInvariant(c));
                default: return true;
            }
        }

        // Pasted text: drop what isn't allowed. Money keeps one dot, 2 decimals, and 8 digits before the dot (₱99,999,999.99).
        private void Clean()
        {
            string text = Input.Text;
            if (Kind == InputKind.Any)
            {
                if (text.Length > MaxLength) { Input.Text = text.Substring(0, MaxLength); Input.SelectionStart = MaxLength; }
                return;
            }
            string clean = Kind == InputKind.Username ? text.ToLowerInvariant() : text;
            clean = new string(clean.Where(c => Kind == InputKind.Money ? char.IsDigit(c) || c == '.' : Allowed(c)).ToArray());
            if (Kind == InputKind.Money)
            {
                int dot = clean.IndexOf('.');
                string whole = dot < 0 ? clean : clean.Substring(0, dot);
                string cents = dot < 0 ? "" : new string(clean.Substring(dot + 1).Where(char.IsDigit).Take(2).ToArray());
                if (whole.Length > 8) whole = whole.Substring(0, 8);
                clean = dot < 0 ? whole : whole + "." + cents;
            }
            if (Kind == InputKind.Name) { clean = clean.TrimStart(); while (clean.Contains("  ")) clean = clean.Replace("  ", " "); }
            if (clean.Length > MaxLength) clean = clean.Substring(0, MaxLength);
            if (clean == text) return;
            int caret = Math.Max(0, Input.SelectionStart - (text.Length - clean.Length));
            Input.Text = clean;
            Input.SelectionStart = Math.Min(caret, clean.Length);
        }

        // Puts the typing cursor in this field.
        public void FocusInput()
        {
            Input.Focus();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Input.Font = Font;
        }

        public void ApplyColors()
        {
            BackColor = Ui.InputBackground;
            Input.BackColor = Ui.InputBackground;
            Input.ForeColor = Ui.Colors.TextPrimary;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Ui.Rounded(box, 10))
            using (SolidBrush fill = new SolidBrush(BackColor))
            using (Pen border = new Pen(Input.Focused ? Ui.Colors.Primary : Ui.Colors.BorderPrimary))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }
            if (_prefix != "")
                TextRenderer.DrawText(g, _prefix, Ui.UiFont(10.5F, true), new Rectangle(12, 0, 20, Height), Ui.Colors.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            PaintExtras(g);
        }

        // Lets a field draw something extra inside the box, like the search glass.
        protected virtual void PaintExtras(Graphics g)
        {
        }
    }

    public enum InputKind { Any, Digits, Money, Name, Username }
}

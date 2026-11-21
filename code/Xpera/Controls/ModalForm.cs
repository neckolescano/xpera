using System.Runtime.InteropServices;

namespace Xpera.Controls
{
    // The frame every xpera pop-up uses: a card with a title, a short line under it, a close button,
    // and the buttons along the bottom. While it is open the window behind it dims, so it is clear
    // that this one question needs an answer first. It fades in and out quickly instead of popping.
    public class ModalForm : Form
    {
        public const int Side = 28;                 // space on the left and right inside the card
        private readonly Label _title = new Label();
        private readonly Label _description = new Label();
        private readonly RoundButton _close = new RoundButton();
        private readonly System.Windows.Forms.Timer _fade = new System.Windows.Forms.Timer { Interval = 12 };
        private bool _fadingOut;
        private bool _closeNow;
        private DialogResult _result = DialogResult.None;

        // Where the content can start, just under the title and description.
        protected int ContentTop { get; private set; }
        protected int ContentWidth { get { return ClientSize.Width - Side * 2; } }

        public ModalForm(string title, string description, int width = 460)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            KeyPreview = true;
            DoubleBuffered = true;
            Opacity = 0;
            BackColor = Ui.Colors.BackgroundPrimary;
            Font = Ui.UiFont(9.5F, false);
            ClientSize = new Size(width, 200);
            Text = title;

            _title.Text = title;
            _title.Font = Ui.UiFont(14F, true);
            _title.ForeColor = Ui.Colors.TextPrimary;
            _title.SetBounds(Side, 24, width - Side * 2 - 40, 30);
            _title.Tag = "keep";
            Controls.Add(_title);

            int top = _title.Bottom;
            if (description != "")
            {
                _description.Text = description;
                _description.Font = Ui.UiFont(9.5F, false);
                _description.ForeColor = Ui.Colors.TextSecondary;
                _description.Tag = "keep";
                int height = TextRenderer.MeasureText(description, _description.Font, new Size(width - Side * 2, 0), TextFormatFlags.WordBreak).Height;
                _description.SetBounds(Side, top + 4, width - Side * 2, height + 4);
                Controls.Add(_description);
                top = _description.Bottom;
            }
            ContentTop = top + 22;

            // A small round × in the top right corner. It does the same as Cancel or Esc.
            _close.Text = "×";
            _close.Font = Ui.UiFont(13F, false);
            _close.Radius = 16;
            _close.SetBounds(width - Side - 26, 22, 32, 32);
            _close.BackColor = Ui.Colors.BackgroundPrimary;
            _close.ForeColor = Ui.Colors.TextSecondary;
            _close.FlatAppearance.BorderColor = Ui.Colors.BackgroundPrimary;
            _close.Tag = "keep";
            _close.TabStop = false;
            _close.Click += (sender, e) => { DialogResult = DialogResult.Cancel; };
            Controls.Add(_close);

            _fade.Tick += (sender, e) => StepFade();
        }

        // A primary (red) or secondary button for the bottom row.
        protected RoundButton MakeButton(string text, bool primary)
        {
            RoundButton button = new RoundButton { Text = text, Height = Sizes.Button, Radius = Sizes.Radius };
            if (primary) Ui.StylePrimary(button); else Ui.StyleSecondary(button);
            button.Font = Ui.UiFont(9.5F, true);
            button.Width = Math.Max(primary ? 140 : 100, TextRenderer.MeasureText(text, button.Font).Width + 44);
            return button;
        }

        // Places the buttons at the bottom right (primary last), under a thin line, and sets the card's height.
        protected void PlaceButtons(int top, params Button[] buttons)
        {
            int y = top + 20;
            int x = ClientSize.Width - Side;
            foreach (Button button in buttons.Reverse())
            {
                x -= button.Width;
                button.Location = new Point(x, y);
                Controls.Add(button);
                x -= 10;
            }
            ClientSize = new Size(ClientSize.Width, y + Sizes.Button + 22);
            _footerLine = top;
        }

        private int _footerLine = -1;

        // Opens over the window that owns it, with that window dimmed behind.
        public DialogResult ShowModal(Control owner)
        {
            Form parent = owner?.FindForm();
            using Form dim = new Form
            {
                FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual,
                BackColor = Color.Black, Opacity = 0.45
            };
            if (parent != null)
            {
                dim.Bounds = parent.RectangleToScreen(parent.ClientRectangle);
                dim.Show(parent);
                Location = new Point(dim.Left + (dim.Width - Width) / 2, dim.Top + Math.Max(24, (dim.Height - Height) / 2 - 20));
            }
            else
            {
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(screen.Left + (screen.Width - Width) / 2, screen.Top + (screen.Height - Height) / 2);
            }
            DialogResult result = parent != null ? ShowDialog(dim) : ShowDialog();
            dim.Close();
            return result;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _fade.Start();
        }

        private void StepFade()
        {
            double next = Opacity + (_fadingOut ? -0.2 : 0.2);
            if (!_fadingOut && next >= 1) { Opacity = 1; _fade.Stop(); return; }
            if (_fadingOut && next <= 0)
            {
                _fade.Stop();
                _closeNow = true;
                if (_result != DialogResult.None) DialogResult = _result; else Close();
                return;
            }
            Opacity = next;
        }

        // Fades out first, then really closes.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_closeNow && Visible && Opacity > 0)
            {
                // Remember the answer; Windows clears it when the close is put on hold.
                _result = DialogResult;
                e.Cancel = true;
                if (!_fadingOut) { _fadingOut = true; _fade.Start(); }
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; e.Handled = true; return; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using Pen border = new Pen(Ui.Colors.BorderPrimary);
            e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            if (_footerLine >= 0)
            {
                using Pen line = new Pen(Ui.Colors.BorderSecondary);
                e.Graphics.DrawLine(line, 0, _footerLine, Width, _footerLine);
            }
        }

        // Rounded corners on Windows 11 and the small shadow Windows gives pop-ups.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return p;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int round = 2; // DWMWCP_ROUND
            try { DwmSetWindowAttribute(Handle, 33, ref round, sizeof(int)); } catch (DllNotFoundException) { }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    }
}

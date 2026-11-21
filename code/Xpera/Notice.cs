using System.Drawing.Drawing2D;
using Xpera.Controls;
using Xpera.Theme;

namespace Xpera
{
    public enum NoticeKind { Info, Success, Warning }

    // xpera's own message box, in the same frame as every other pop-up, so nothing looks like a plain
    // Windows box. Show tells someone what happened; Confirm asks before something that can't be undone.
    public class Notice : ModalForm
    {
        private readonly NoticeKind _kind;
        private readonly Rectangle _badge;

        private Notice(string title, string message, NoticeKind kind, string okText, string cancelText)
            : base(title, "", 440)
        {
            _kind = kind;
            // The badge sits on the left of the message.
            _badge = new Rectangle(Side, ContentTop - 6, 40, 40);
            Label text = new Label { Text = message, Font = Ui.UiFont(10F, false), ForeColor = Ui.Colors.TextPrimary, Tag = "keep" };
            int width = ContentWidth - 56;
            int height = TextRenderer.MeasureText(message, text.Font, new Size(width, 0), TextFormatFlags.WordBreak).Height + 4;
            text.SetBounds(Side + 56, ContentTop - 6 + Math.Max(0, (40 - height) / 2), width, height);
            Controls.Add(text);

            RoundButton ok = MakeButton(okText, true);
            ok.DialogResult = DialogResult.OK;
            AcceptButton = ok;
            int bottom = Math.Max(text.Bottom, _badge.Bottom) + 24;
            if (cancelText != null)
            {
                RoundButton cancel = MakeButton(cancelText, false);
                cancel.DialogResult = DialogResult.Cancel;
                CancelButton = cancel;
                PlaceButtons(bottom, cancel, ok);
            }
            else
            {
                PlaceButtons(bottom, ok);
            }
        }

        public static void Show(Control owner, string title, string message, NoticeKind kind = NoticeKind.Info)
        {
            using Notice notice = new Notice(title, message, kind, "OK", null);
            notice.ShowModal(owner);
        }

        public static bool Confirm(Control owner, string title, string message, string confirmText)
        {
            using Notice notice = new Notice(title, message, NoticeKind.Warning, confirmText, "Cancel");
            return notice.ShowModal(owner) == DialogResult.OK;
        }

        // A round badge: a check for done, an exclamation mark for a problem, an "i" for a note.
        // Problems use the soft red the status tags use, so the meaning matches across the app.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            bool warn = _kind == NoticeKind.Warning;
            Color fill = warn ? (Ui.IsDarkMode ? Shades.SignalRed.VeryDarkRed : Tints.SignalRed.MistyRose) : (Ui.IsDarkMode ? Tints.Carbon.DarkCharcoal : Tints.Carbon.VeryLightGray);
            Color ink = warn ? (Ui.IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson) : Ui.Colors.TextPrimary;
            using (SolidBrush brush = new SolidBrush(fill)) g.FillEllipse(brush, _badge);
            using Pen pen = new Pen(ink, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            int cx = _badge.X + 20, cy = _badge.Y + 20;
            if (_kind == NoticeKind.Success)
            {
                g.DrawLines(pen, new[] { new Point(cx - 7, cy), new Point(cx - 2, cy + 5), new Point(cx + 8, cy - 6) });
            }
            else if (warn)
            {
                g.DrawLine(pen, cx, cy - 8, cx, cy + 2);
                g.DrawLine(pen, cx, cy + 7, cx, cy + 7.5f);
            }
            else
            {
                g.DrawLine(pen, cx, cy - 2, cx, cy + 8);
                g.DrawLine(pen, cx, cy - 8, cx, cy - 7.5f);
            }
        }
    }
}

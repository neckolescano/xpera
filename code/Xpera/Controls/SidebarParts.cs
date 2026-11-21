using System.Drawing.Drawing2D;
using Xpera.Theme;

namespace Xpera.Controls
{
    // The card at the bottom of the sidebar: who is signed in, and which store and plan this is.
    // A cashier sharing a counter PC can see at a glance whose account is open.
    public class ProfileCard : Control
    {
        public string UserName = "";
        public string Role = "";
        public string Store = "";
        public string Plan = "";
        public float SizeScale = 1f;       // size of the boxes (window size × display scaling)
        public float TextScale = 1f;   // size of the text (window size only)

        // The small sign-out arrow in the top right corner of the card.
        public event EventHandler SignOutClicked;
        private Rectangle _signOutBox;
        private bool _overSignOut;
        private readonly ToolTip _tip = new ToolTip();

        public ProfileCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 108;
            AccessibleName = "Signed-in user";
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool over = _signOutBox.Contains(e.Location);
            if (over != _overSignOut)
            {
                _overSignOut = over;
                Cursor = over ? Cursors.Hand : Cursors.Default;
                if (over) _tip.Show("Sign out", this, _signOutBox.Left, _signOutBox.Bottom + 4, 1500); else _tip.Hide(this);
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_overSignOut) { _overSignOut = false; Cursor = Cursors.Default; _tip.Hide(this); Invalidate(); }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _signOutBox.Contains(e.Location)) SignOutClicked?.Invoke(this, EventArgs.Empty);
            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Ui.SidebarBackground);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle box = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Ui.Rounded(box, 12))
            using (SolidBrush fill = new SolidBrush(Tones.Carbon.VeryDarkGray))
            using (Pen border = new Pen(Tints.Carbon.DarkCharcoal))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            // Initials in a Bone circle, like the customer avatar.
            float s = SizeScale;
            float t = TextScale;   // fonts already follow display scaling, so text only grows with the window
            int S(float v) => (int)Math.Round(v * s);
            // In the Designer no one is signed in yet, so it shows sample text.
            string UserName = this.UserName == "" && DesignMode ? "Signed-in user" : this.UserName;
            string Role = this.Role == "" && DesignMode ? "Role" : this.Role;
            string Store = this.Store == "" && DesignMode ? "Store · Branch" : this.Store;
            string Plan = this.Plan == "" && DesignMode ? "Tenant · Plan" : this.Plan;
            Rectangle circle = new Rectangle(S(12), S(12), S(36), S(36));
            using (SolidBrush dot = new SolidBrush(Ui.Bone)) g.FillEllipse(dot, circle);
            string initials = string.Concat(UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpper(w[0])));
            TextRenderer.DrawText(g, initials, Typography.Get(9.5F * t, Weight.SemiBold), circle, ColorTranslator.FromHtml("#121212"), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            TextRenderer.DrawText(g, UserName, Typography.Get(10F * t, Weight.SemiBold), new Rectangle(S(58), S(12), Width - S(94), S(20)), Tints.Carbon.White, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Role, Typography.Scaled(Typography.Meta, t), new Rectangle(S(58), S(31), Width - S(94), S(18)), Tints.Carbon.Gray, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            // Sign out: an open bracket (the door) with an arrow walking out of it.
            _signOutBox = new Rectangle(Width - S(38), S(14), S(28), S(28));
            if (_overSignOut)
                using (GraphicsPath hover = Ui.Rounded(_signOutBox, S(8)))
                using (SolidBrush fill = new SolidBrush(Tints.Carbon.DarkCharcoal)) g.FillPath(fill, hover);
            Color door = _overSignOut ? Tints.SignalRed.SoftRose : Tints.Carbon.LightGray;
            using (Pen pen = new Pen(door, 1.6f * s) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                int x = _signOutBox.X + S(6), y = _signOutBox.Y + S(6);
                g.DrawLines(pen, new[] { new Point(x + S(7), y), new Point(x, y), new Point(x, y + S(16)), new Point(x + S(7), y + S(16)) });
                g.DrawLine(pen, x + S(6), y + S(8), x + S(16), y + S(8));
                g.DrawLines(pen, new[] { new Point(x + S(12), y + S(4)), new Point(x + S(16), y + S(8)), new Point(x + S(12), y + S(12)) });
            }

            using (Pen line = new Pen(Tints.Carbon.DarkCharcoal)) g.DrawLine(line, S(12), S(58), Width - S(12), S(58));

            // The store, then its plan as a small tag.
            TextRenderer.DrawText(g, Store, Typography.Scaled(Typography.Meta, t), new Rectangle(S(12), S(65), Width - S(24), S(18)), Tints.Carbon.LightGray, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            Font small = Typography.Get(8F * t, Weight.SemiBold);
            Size size = TextRenderer.MeasureText(Plan.ToUpper(), small);
            Rectangle tag = new Rectangle(S(12), S(85), size.Width + S(12), S(18));
            using (GraphicsPath pill = Ui.Rounded(tag, tag.Height / 2))
            using (SolidBrush fill = new SolidBrush(Shades.SignalRed.VeryDarkRed)) g.FillPath(fill, pill);
            TextRenderer.DrawText(g, Plan.ToUpper(), small, tag, Tints.SignalRed.SoftRose, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    // "Back up now" with a cloud and an up arrow, and when the last backup was on the line under it.
    // Only the Admin (store owner) sees it. Signing out also backs up by itself.
    public class BackupRow : Control
    {
        private bool _hover;
        public string Status = "";          // like "Last backup · Nov 17, 5:32 PM"
        public bool Failed;                 // the last try did not work, so the line turns red
        public bool Busy;                   // while a backup runs
        public float SizeScale = 1f;
        public float TextScale = 1f;

        public BackupRow()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 52;
            Cursor = Cursors.Hand;
            AccessibleName = "Back up to the cloud";
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Ui.SidebarBackground);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hover && !Busy)
            {
                using GraphicsPath path = Ui.Rounded(new Rectangle(0, 0, Width - 1, Height - 1), 8);
                using SolidBrush fill = new SolidBrush(Tones.Carbon.VeryDarkGray);
                g.FillPath(fill, path);
            }

            float s = SizeScale;
            float t = TextScale;
            int S(float v) => (int)Math.Round(v * s);
            Color ink = Tints.Carbon.LightGray;

            // A cloud outline (small bump, big bump, small bump, flat bottom) with an arrow going up into it.
            int x = S(16), bottom = Height / 2 + S(7);
            using (GraphicsPath cloud = new GraphicsPath())
            using (Pen pen = new Pen(ink, 1.6f * s) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                cloud.AddArc(x, bottom - S(8), S(8), S(8), 90, 180);
                cloud.AddArc(x + S(4), bottom - S(15), S(12), S(12), 180, 180);
                cloud.AddArc(x + S(12), bottom - S(9), S(9), S(9), 270, 180);
                cloud.CloseFigure();
                g.DrawPath(pen, cloud);
                int cx = x + S(10);
                g.DrawLine(pen, cx, bottom + S(4), cx, bottom - S(6));
                g.DrawLines(pen, new[] { new Point(cx - S(3), bottom - S(3)), new Point(cx, bottom - S(6)), new Point(cx + S(3), bottom - S(3)) });
            }

            string status = Status == "" && DesignMode ? "Last backup · today" : Status;
            TextRenderer.DrawText(g, Busy ? "Backing up…" : "Back up now", Typography.Scaled(Typography.Nav, t),
                new Rectangle(S(52), S(7), Width - S(60), Height / 2 - S(4)), ink, TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, status, Typography.Scaled(Typography.Meta, t),
                new Rectangle(S(52), Height / 2 + S(1), Width - S(60), Height / 2 - S(6)), Failed ? Tints.SignalRed.SoftRose : Tints.Carbon.Gray,
                TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
        }
    }

    // "Dark mode" with a small switch, part of the sidebar instead of a boxed button.
    // The switch knob slides right and turns red when dark mode is on.
    public class ThemeToggle : Control
    {
        private bool _hover;
        public float SizeScale = 1f;       // size of the boxes (window size × display scaling)
        public float TextScale = 1f;   // size of the text (window size only)

        public ThemeToggle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 42;
            Cursor = Cursors.Hand;
            AccessibleName = "Dark mode switch";
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Ui.SidebarBackground);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hover)
            {
                using GraphicsPath path = Ui.Rounded(new Rectangle(0, 0, Width - 1, Height - 1), 8);
                using SolidBrush fill = new SolidBrush(Tones.Carbon.VeryDarkGray);
                g.FillPath(fill, path);
            }

            float s = SizeScale;
            float t = TextScale;   // fonts already follow display scaling, so text only grows with the window
            int S(float v) => (int)Math.Round(v * s);

            // A crescent moon: a circle with a bite taken out by a second circle in the background color.
            Color ink = Tints.Carbon.LightGray;
            using (Pen pen = new Pen(ink, 1.6f * s)) g.DrawEllipse(pen, S(18), Height / 2 - S(8), S(16), S(16));
            using (SolidBrush bite = new SolidBrush(_hover ? Tones.Carbon.VeryDarkGray : Ui.SidebarBackground)) g.FillEllipse(bite, S(24), Height / 2 - S(12), S(14), S(14));

            TextRenderer.DrawText(g, "Dark mode", Typography.Scaled(Typography.Nav, t), new Rectangle(S(52), 0, Width - S(112), Height), ink, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            Rectangle track = new Rectangle(Width - S(54), Height / 2 - S(11), S(40), S(22));
            using (GraphicsPath path = Ui.Rounded(track, track.Height / 2))
            using (SolidBrush fill = new SolidBrush(Ui.IsDarkMode ? Ui.Colors.Primary : Tones.Carbon.IronGray)) g.FillPath(fill, path);
            int k = track.Height - S(6);
            Rectangle knob = new Rectangle(Ui.IsDarkMode ? track.Right - k - S(3) : track.X + S(3), track.Y + S(3), k, k);
            using (SolidBrush white = new SolidBrush(Tints.Carbon.White)) g.FillEllipse(white, knob);
        }
    }
}

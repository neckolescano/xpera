using System.Drawing.Drawing2D;
using Xpera.Controls;
using Xpera.Theme;

namespace Xpera
{
    // Shared look for every screen, so all of them match the brand.
    public static class Ui
    {
        // Bone is the brand's background-only neutral. It is declared first because
        // the light palette below uses it, and C# sets these fields from top to bottom.
        public static readonly Color Bone = ColorTranslator.FromHtml("#F4F1EA");

        // The colors in use right now. Dark mode swaps in a second palette.
        public static Palette Colors { get; private set; } = LightPalette();
        public static bool IsDarkMode { get; private set; }

        // Three layers, darkest to lightest in dark mode and the reverse in light mode:
        // sidebar, page, then cards on top. Each layer is a different brand color, so they never blend.
        //   Light: sidebar Carbon #121212, page Carbon 90% tint #E7E7E7, cards Bone #F4F1EA, fields white.
        //   Dark:  sidebar Carbon 70% shade #050505, page Carbon #121212, cards Carbon 10% tone #1D1D1D.
        public static Color PageBackground
        {
            get { return IsDarkMode ? Colors.Secondary : Tints.Carbon.VeryLightGray; }
        }

        public static Color SidebarBackground
        {
            get { return IsDarkMode ? Shades.Carbon.PitchBlack : Colors.Secondary; }
        }

        // Light mode keeps the Palette's colors, but cards use Bone instead of pure white,
        // which is softer on the eyes during a long shift at the counter.
        private static Palette LightPalette()
        {
            Palette light = new Palette();
            light.BackgroundPrimary = Bone;
            return light;
        }

        // Same Palette class, different values, set with its setters.
        private static Palette DarkPalette()
        {
            Palette dark = new Palette();
            dark.TextPrimary = Tints.Carbon.White;
            dark.TextSecondary = Tints.Carbon.SilverGray;
            dark.BorderPrimary = Tones.Carbon.Charcoal;
            dark.BorderSecondary = Tones.Carbon.DarkOnyx;
            dark.BackgroundPrimary = Tones.Carbon.VeryDarkGray;
            dark.BackgroundSecondary = Shades.SignalRed.VeryDarkRed;
            return dark;
        }

        public static void SetDarkMode(bool on)
        {
            IsDarkMode = on;
            Colors = on ? DarkPalette() : LightPalette();
        }

        // Text boxes and dropdowns sit a little lighter than the card in dark mode.
        public static Color InputBackground
        {
            get { return IsDarkMode ? Tones.Carbon.DarkOnyx : Tints.Carbon.White; }
        }

        // ---------- type ----------
        // All text is Inter, from the type scale in Typography.cs. This older "size and bold" helper
        // snaps to the nearest step on that scale, so every call lands on the same few sizes.
        public static Font UiFont(float size, bool strong)
        {
            return Typography.FromOld(size, strong);
        }

        // Runs UseBestFont on a form and everything inside it. ApplyTheme already does this for screens.
        public static void UseBestFonts(Control parent)
        {
            UseBestFont(parent);
            foreach (Control control in parent.Controls) UseBestFonts(control);
        }

        // Swaps a Designer font (Segoe UI) for the matching Inter step on the type scale.
        private static void UseBestFont(Control control)
        {
            Font font = control.Font;
            if (control is DataGridView) return; // Tables get their fonts once, in StyleGrid.
            if (Typography.IsInter(font)) return;
            control.Font = Typography.FromOld(font.Size, font.Name.Contains("Semibold") || font.Bold);
        }

        // Repaints a form or screen and everything inside it with the current colors.
        // Tags: "keep" = leave alone, "muted" = gray text, "note" = red note, "heading" = bold title.
        public static void ApplyTheme(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (Equals(control.Tag, "keep")) { UseBestFont(control); continue; }
                UseBestFont(control);

                if (control is Card card) card.ApplyColors();
                else if (control is InputBox input) { input.ApplyColors(); continue; }
                else if (control is DateField date) date.Invalidate();
                else if (control is Pager pager) pager.Restyle();
                else if (control is StatStrip strip) strip.Invalidate();
                else if (control is MiniStats mini) mini.Invalidate();
                else if (control is DataGridView grid) { StyleGridColors(grid); grid.Invalidate(); continue; }
                else if (control is Button button && Equals(button.Tag, "primary")) StylePrimary(button);
                else if (control is Button link && Equals(link.Tag, "link")) StyleLink(link);
                else if (control is Button dashed && Equals(dashed.Tag, "dashed")) StyleDashed(dashed);
                else if (control is Button other) StyleSecondary(other);
                else if (control is TextBox || control is ComboBox)
                {
                    // Windows only lets a dropdown change color when it is flat.
                    if (control is ComboBox dropdown) dropdown.FlatStyle = FlatStyle.Flat;
                    control.BackColor = InputBackground;
                    control.ForeColor = Colors.TextPrimary;
                }
                else if (control is Label && Equals(control.Tag, "note"))
                {
                    control.BackColor = Colors.BackgroundSecondary;
                    control.ForeColor = IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson;
                }
                else if (control is Label || control is RadioButton)
                {
                    control.ForeColor = Equals(control.Tag, "muted") ? Colors.TextSecondary : Colors.TextPrimary;
                }

                ApplyTheme(control);
            }
        }

        // The peso format lives in Xpera.domain (Money), so the database code can use it too.
        public static string Peso(decimal amount) => Money.Peso(amount);

        public static decimal ParsePeso(string text) => Money.ParsePeso(text);

        public static GraphicsPath Rounded(Rectangle box, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(box.X, box.Y, d, d, 180, 90);
            path.AddArc(box.Right - d, box.Y, d, d, 270, 90);
            path.AddArc(box.Right - d, box.Bottom - d, d, d, 0, 90);
            path.AddArc(box.X, box.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void StylePrimary(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            // The Enter-key button gets a border anyway, so match it to the button.
            button.FlatAppearance.BorderColor = Colors.Primary;
            button.BackColor = Colors.Primary;
            button.ForeColor = Colors.Secondary;
            button.Font = Typography.Button;
            button.Cursor = Cursors.Hand;
            button.Tag = "primary";
        }

        // Red text with no box, like "Change" next to the customer.
        public static void StyleLink(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Color.Empty;
            button.BackColor = button.Parent != null ? button.Parent.BackColor : PageBackground;
            button.ForeColor = IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson;
            button.Font = UiFont(9.5F, true);
            button.Cursor = Cursors.Hand;
        }

        // A dashed outline, for a spot where you add something.
        public static void StyleDashed(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Colors.BorderPrimary;
            button.BackColor = Colors.BackgroundPrimary;
            button.ForeColor = Colors.TextSecondary;
            button.Cursor = Cursors.Hand;
            if (button is RoundButton round) round.Dashed = true;
        }

        public static void StyleSecondary(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = button is RoundButton ? 0 : 1;
            button.FlatAppearance.BorderColor = IsDarkMode ? Tones.Carbon.Graphite : Tints.Carbon.LightGray;
            button.BackColor = Colors.BackgroundPrimary;
            button.ForeColor = Colors.TextPrimary;
            button.Cursor = Cursors.Hand;
        }

        // One half of a two-choice switch, like Cash / Credit. The chosen half is Carbon.
        public static void StyleChoice(Button button, bool chosen)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = UiFont(9.5F, true);
            button.FlatAppearance.BorderColor = Color.Empty;
            button.Tag = "keep";
            // The unchosen half takes its container's color, so only the chosen half stands out.
            Color container = button.Parent != null ? button.Parent.BackColor : PageBackground;
            button.BackColor = chosen ? (IsDarkMode ? Bone : Colors.Secondary) : container;
            button.ForeColor = chosen ? (IsDarkMode ? Colors.Secondary : Bone) : Colors.TextSecondary;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;

            // Tables draw into memory first and then show the finished picture, so no half-drawn rows or
            // leftovers from the previous tab or mode can show between cells. Windows keeps this switch private.
            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(grid, true);

            // Windows leaves a 1-pixel gap between cells unpainted even with no cell borders, and whatever was
            // there before (the previous tab, or the old colors after a light/dark switch) showed through as thin
            // lines. Painting the whole row first, gaps included, fixes it for every table.
            grid.RowPrePaint += (sender, e) =>
            {
                bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
                using SolidBrush back = new SolidBrush(selected ? Colors.BackgroundSecondary : Colors.BackgroundPrimary);
                e.Graphics.FillRectangle(back, new Rectangle(0, e.RowBounds.Y, grid.Width, e.RowBounds.Height));
            };

            // The table draws no cell borders of its own. One thin line is drawn under each row instead,
            // so lines between columns can never show up, even after switching light and dark mode.
            grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
            grid.RowPostPaint += (sender, e) =>
            {
                int y = e.RowBounds.Bottom - 1;
                // The first cell leaves a 1-pixel edge on the left; cover it with the row's own color.
                bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
                using (SolidBrush back = new SolidBrush(selected ? Colors.BackgroundSecondary : Colors.BackgroundPrimary))
                    e.Graphics.FillRectangle(back, 0, e.RowBounds.Y, e.RowBounds.Left + 2, e.RowBounds.Height - 1);
                using (Pen line = new Pen(Colors.BorderSecondary)) e.Graphics.DrawLine(line, e.RowBounds.Left, y, e.RowBounds.Right, y);
            };
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            // Rows a little taller than the text, so a long list is easy to follow across.
            float dpi = grid.DeviceDpi / 96f;
            grid.ColumnHeadersHeight = (int)(Sizes.TableHeader * dpi);
            grid.RowTemplate.Height = (int)(Sizes.TableRow * dpi);
            // Only a sideways scrollbar, and only when the window is too narrow for every column.
            // Rows never scroll inside the table: the card grows to fit its page of rows instead.
            grid.ScrollBars = ScrollBars.Horizontal;
            foreach (DataGridViewColumn column in grid.Columns) column.MinimumWidth = (int)(80 * dpi);  // columns from the Designer
            grid.ColumnAdded += (sender, e) => e.Column.MinimumWidth = (int)(80 * dpi);
            grid.ColumnHeadersDefaultCellStyle.Font = Typography.TableHeader;
            grid.DefaultCellStyle.Font = Typography.Table;
            grid.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 4, 0);
            StyleGridColors(grid);
        }

        public static void StyleGridColors(DataGridView grid)
        {
            grid.BackgroundColor = Colors.BackgroundPrimary;
            // After a light/dark switch, Windows sometimes still draws cell borders in the grid color
            // (thin gray lines between columns). Giving the grid the card's own color makes them invisible.
            grid.GridColor = Colors.BackgroundPrimary;

            // New style objects each time (instead of editing the old ones), so the table repaints cleanly.
            DataGridViewCellStyle header = new DataGridViewCellStyle(grid.ColumnHeadersDefaultCellStyle);
            header.BackColor = Colors.BackgroundPrimary;
            header.ForeColor = Colors.TextSecondary;
            header.SelectionBackColor = Colors.BackgroundPrimary;
            header.SelectionForeColor = Colors.TextSecondary;
            grid.ColumnHeadersDefaultCellStyle = header;

            DataGridViewCellStyle cells = new DataGridViewCellStyle(grid.DefaultCellStyle);
            cells.BackColor = Colors.BackgroundPrimary;
            cells.ForeColor = Colors.TextPrimary;
            cells.SelectionBackColor = Colors.BackgroundSecondary;
            cells.SelectionForeColor = Colors.TextPrimary;
            grid.DefaultCellStyle = cells;
        }

        // Turns a column placed in the Designer into a dropdown, used for things like Status. It looks like a
        // status tag with a small chevron; clicking it opens the brand menu (not Windows' own dropdown), so it
        // matches every other list in xpera. onChanged runs with the record's place in the full list
        // (saved in the row's Tag) and the new value.
        public static void MakeDropdown(DataGridView grid, string name, string[] choices, Action<int, string> onChanged)
        {
            _dropdowns.Add((grid, name));

            grid.CellPainting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != name) return;
                e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                DrawTag(e.Graphics, e.CellBounds, Convert.ToString(e.Value), true);
                e.Handled = true;
            };
            grid.CellMouseEnter += (sender, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0) grid.Cursor = grid.Columns[e.ColumnIndex].Name == name ? Cursors.Hand : Cursors.Default;
            };
            grid.CellClick += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != name) return;
                DataGridViewRow row = grid.Rows[e.RowIndex];
                string current = Convert.ToString(row.Cells[name].Value);
                Rectangle cell = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);

                BrandMenu menu = new BrandMenu();
                foreach (string choice in choices)
                {
                    string value = choice;
                    menu.AddItem(value == current ? value + "   ✓" : value, true, () =>
                    {
                        if (value == current) return;
                        row.Cells[name].Value = value;
                        if (row.Tag is int index) onChanged(index, value);
                    });
                }
                menu.MinimumSize = new Size(Math.Max(cell.Width, 200), 0);
                menu.Show(grid, new Point(cell.X + 2, cell.Bottom - 4));
            };
        }

        private static readonly HashSet<(DataGridView, string)> _dropdowns = new HashSet<(DataGridView, string)>();

        // ---------- row actions (the U and D of CRUD) ----------
        // The last column ("actionsColumn" from the Designer) shows ••• on every row. Clicking it opens a menu
        // like Edit / Delete for that row's record; double-clicking a row runs the first item (Edit).
        // actions gets the record's ID (row.Tag) and returns the menu items for it.
        public static void AddRowActions(DataGridView grid, Func<int, (string Text, Action Run)[]> actions)
        {
            const string name = "actionsColumn";
            DataGridViewColumn column = grid.Columns[name];
            float dpi = grid.DeviceDpi / 96f;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.MinimumWidth = (int)(44 * dpi);
            column.Width = (int)(56 * dpi);
            column.Resizable = DataGridViewTriState.False;

            grid.CellPainting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != name) return;
                e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                using (SolidBrush dot = new SolidBrush(Colors.TextSecondary))
                {
                    int cx = e.CellBounds.X + e.CellBounds.Width / 2, cy = e.CellBounds.Y + e.CellBounds.Height / 2;
                    int r = Math.Max(2, (int)(2 * dpi)), gap = (int)(6 * dpi);
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    for (int i = -1; i <= 1; i++) e.Graphics.FillEllipse(dot, cx + i * gap - r, cy - r, r * 2, r * 2);
                }
                e.Handled = true;
            };
            grid.CellMouseEnter += (sender, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == name) grid.Cursor = Cursors.Hand;
            };
            grid.CellClick += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != name) return;
                if (!(grid.Rows[e.RowIndex].Tag is int id)) return;
                Rectangle cell = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                BrandMenu menu = new BrandMenu();
                foreach ((string Text, Action Run) item in actions(id)) menu.AddItem(item.Text, true, item.Run);
                menu.MinimumSize = new Size((int)(170 * dpi), 0);
                menu.Show(grid, new Point(cell.Right - (int)(170 * dpi), cell.Bottom - 4));
            };
            grid.CellDoubleClick += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                string column = grid.Columns[e.ColumnIndex].Name;
                if (column == name || _dropdowns.Contains((grid, column))) return;
                if (grid.Rows[e.RowIndex].Tag is int id) actions(id).FirstOrDefault().Run?.Invoke();
            };
        }

        // ---------- status tags ----------
        // Brand colors only: done = Carbon tint, needs attention = red outline, problem = red fill.
        private static readonly string[] Done = { "Active", "Resolved", "Available", "Admin", "Synced to cloud" };
        private static readonly string[] Waiting = { "In progress", "Waiting for product", "Due soon", "Credit", "Warranty claim" };
        private static readonly string[] Problem = { "Overdue", "Complaint", "Not available", "Locked", "Cashier", "Not yet synced" };

        // Draws the values in these columns as small rounded tags instead of plain text.
        public static void ShowAsTags(DataGridView grid, params string[] columns)
        {
            grid.CellPainting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || !columns.Contains(grid.Columns[e.ColumnIndex].Name)) return;
                string text = Convert.ToString(e.Value);
                if (text == "") return;

                e.PaintBackground(e.CellBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                DrawTag(e.Graphics, e.CellBounds, text, false);
                e.Handled = true;
            };
        }

        // One rounded status tag inside a table cell. With a chevron, it reads as something you can change.
        public static void DrawTag(Graphics g, Rectangle cell, string text, bool chevron)
        {
            if (text == "") return;
            Font font = UiFont(8.5F, true);
            Size size = TextRenderer.MeasureText(text, font);
            int extra = chevron ? 18 : 0;
            // A tag never grows past its column; long text ends with "…" instead of spilling over.
            Rectangle tag = new Rectangle(cell.X + 6, cell.Y + (cell.Height - 24) / 2, Math.Min(size.Width + 12 + extra, cell.Width - 12), 24);

            Color fill, ink, line;
            if (Done.Contains(text)) { fill = IsDarkMode ? Tints.Carbon.DarkCharcoal : Tints.Carbon.VeryLightGray; ink = Colors.TextPrimary; line = fill; }
            else if (Waiting.Contains(text)) { fill = Colors.BackgroundPrimary; ink = IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson; line = IsDarkMode ? Shades.SignalRed.DeepBurgundy : Tints.SignalRed.LightPink; }
            else if (Problem.Contains(text) || text == "Open") { fill = IsDarkMode ? Shades.SignalRed.VeryDarkRed : Tints.SignalRed.MistyRose; ink = IsDarkMode ? Tints.SignalRed.SoftRose : Shades.SignalRed.Crimson; line = fill; }
            else { fill = Colors.BackgroundPrimary; ink = Colors.TextSecondary; line = Colors.BorderPrimary; }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = Rounded(tag, 11))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(line))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
            TextRenderer.DrawText(g, text, font, new Rectangle(tag.X + 4, tag.Y, tag.Width - extra - 8, tag.Height), ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (chevron)
            {
                using Pen arrow = new Pen(ink, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                int cx = tag.Right - 13, cy = tag.Y + 12;
                g.DrawLines(arrow, new[] { new Point(cx - 3, cy - 1), new Point(cx, cy + 2), new Point(cx + 3, cy - 1) });
            }
        }
    }
}

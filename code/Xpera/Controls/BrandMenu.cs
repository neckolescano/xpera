namespace Xpera.Controls
{
    // A small pop-up menu in the brand colors, used for the status choices in tables.
    public class BrandMenu : ContextMenuStrip
    {
        public BrandMenu()
        {
            Font = Ui.UiFont(9.5F, false);
            ShowImageMargin = false;
            Padding = new Padding(6);
            BackColor = Ui.Colors.BackgroundPrimary;
            ForeColor = Ui.Colors.TextPrimary;
            Renderer = new ToolStripProfessionalRenderer(new BrandColors()) { RoundedEdges = true };
        }

        public ToolStripMenuItem AddItem(string text, bool enabled, Action onClick)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Enabled = enabled;
            item.Padding = new Padding(4, 7, 4, 7);
            item.ForeColor = enabled ? Ui.Colors.TextPrimary : Ui.Colors.TextSecondary;
            item.Click += (sender, e) => onClick();
            Items.Add(item);
            return item;
        }

        private class BrandColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return Ui.Colors.BackgroundPrimary; } }
            public override Color MenuBorder { get { return Ui.Colors.BorderPrimary; } }
            public override Color MenuItemBorder { get { return Ui.Colors.BackgroundSecondary; } }
            public override Color MenuItemSelected { get { return Ui.Colors.BackgroundSecondary; } }
            public override Color ImageMarginGradientBegin { get { return Ui.Colors.BackgroundPrimary; } }
            public override Color ImageMarginGradientMiddle { get { return Ui.Colors.BackgroundPrimary; } }
            public override Color ImageMarginGradientEnd { get { return Ui.Colors.BackgroundPrimary; } }
            public override Color CheckBackground { get { return Ui.Colors.BackgroundSecondary; } }
            public override Color CheckSelectedBackground { get { return Ui.Colors.BackgroundSecondary; } }
        }
    }
}

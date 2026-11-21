namespace Xpera
{
    // The shared sizes for controls, next to the type scale in Typography.cs, so every field, button,
    // and table row has the same height everywhere. Sizes are pixels at 100% Windows display scaling;
    // use Px for anything set in code, so it also fits 125% or 150% screens.
    public static class Sizes
    {
        public const int Field = 40;        // text fields and dropdowns
        public const int Button = 38;       // buttons in forms and pop-ups
        public const int Radius = 10;       // corners of fields and buttons (cards use 14)
        public const int TableRow = 40;     // one row in a table
        public const int TableHeader = 36;  // the column names row
        public const int MenuItem = 40;     // one sidebar item
        public const int PickerRow = 48;    // one row in a pop-up list (name and a gray line)
        public const int PerPage = 8;       // rows on one page of a list

        // A size at 100% made right for the screen the control is on (125% makes 40 into 50).
        public static int Px(Control control, float value)
        {
            return (int)Math.Round(value * control.DeviceDpi / 96f);
        }
    }
}

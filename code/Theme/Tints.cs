using System.Drawing;

namespace Xpera.Theme
{
   
    public static class Tints
    {
    
        public static class SignalRed
        {
            public static Color BrightRose { get; private set; } = ColorTranslator.FromHtml("#F5526E");   // 10%
            public static Color LightRose { get; private set; } = ColorTranslator.FromHtml("#F6657E");    // 20%
            public static Color RosePink { get; private set; } = ColorTranslator.FromHtml("#F7798E");     // 30%
            public static Color SoftRose { get; private set; } = ColorTranslator.FromHtml("#F88C9E");     // 40%
            public static Color PastelPink { get; private set; } = ColorTranslator.FromHtml("#FA9FAF");   // 50%
            public static Color LightPink { get; private set; } = ColorTranslator.FromHtml("#FBB2BF");    // 60%
            public static Color BabyPink { get; private set; } = ColorTranslator.FromHtml("#FCC5CF");     // 70%
            public static Color PalePink { get; private set; } = ColorTranslator.FromHtml("#FDD9DF");     // 80%
            public static Color MistyRose { get; private set; } = ColorTranslator.FromHtml("#FEECEF");    // 90%
            public static Color White { get; private set; } = ColorTranslator.FromHtml("#FFFFFF");        // 100%
        }

        public static class Carbon
        {
            public static Color DarkCharcoal { get; private set; } = ColorTranslator.FromHtml("#2A2A2A");   // 10%
            public static Color DarkGray { get; private set; } = ColorTranslator.FromHtml("#414141");       // 20%
            public static Color DimGray { get; private set; } = ColorTranslator.FromHtml("#595959");        // 30%
            public static Color MediumDarkGray { get; private set; } = ColorTranslator.FromHtml("#717171"); // 40%
            public static Color Gray { get; private set; } = ColorTranslator.FromHtml("#898989");           // 50%
            public static Color MediumGray { get; private set; } = ColorTranslator.FromHtml("#A0A0A0");     // 60%
            public static Color SilverGray { get; private set; } = ColorTranslator.FromHtml("#B8B8B8");     // 70%
            public static Color LightGray { get; private set; } = ColorTranslator.FromHtml("#D0D0D0");      // 80%
            public static Color VeryLightGray { get; private set; } = ColorTranslator.FromHtml("#E7E7E7");  // 90%
            public static Color White { get; private set; } = ColorTranslator.FromHtml("#FFFFFF");          // 100%
        }
    }
}

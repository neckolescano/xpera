using System.Drawing;

namespace Xpera.Theme
{

    public static class Shades
    {
      
        public static class SignalRed
        {
            public static Color DeepRoseRed { get; private set; } = ColorTranslator.FromHtml("#DC3955");  // 10%
            public static Color Crimson { get; private set; } = ColorTranslator.FromHtml("#C3324B");      // 20%
            public static Color DarkCrimson { get; private set; } = ColorTranslator.FromHtml("#AB2C42");  // 30%
            public static Color DeepBurgundy { get; private set; } = ColorTranslator.FromHtml("#922638"); // 40%
            public static Color DarkWine { get; private set; } = ColorTranslator.FromHtml("#7A202F");     // 50%
            public static Color DeepMaroon { get; private set; } = ColorTranslator.FromHtml("#621926");   // 60%
            public static Color DarkOxblood { get; private set; } = ColorTranslator.FromHtml("#49131C");  // 70%
            public static Color VeryDarkRed { get; private set; } = ColorTranslator.FromHtml("#310D13");  // 80%
            public static Color NearBlack { get; private set; } = ColorTranslator.FromHtml("#180609");    // 90%
            public static Color Black { get; private set; } = ColorTranslator.FromHtml("#000000");        // 100%
        }

        public static class Carbon
        {
            public static Color AlmostBlack { get; private set; } = ColorTranslator.FromHtml("#101010");   // 10%
            public static Color RichBlack { get; private set; } = ColorTranslator.FromHtml("#0E0E0E");     // 20%
            public static Color DeepBlack { get; private set; } = ColorTranslator.FromHtml("#0D0D0D");     // 30%
            public static Color CoalBlack { get; private set; } = ColorTranslator.FromHtml("#0B0B0B");     // 40%
            public static Color EbonyBlack { get; private set; } = ColorTranslator.FromHtml("#090909");    // 50%
            public static Color MidnightBlack { get; private set; } = ColorTranslator.FromHtml("#070707"); // 60%
            public static Color PitchBlack { get; private set; } = ColorTranslator.FromHtml("#050505");    // 70%
            public static Color DarkestBlack { get; private set; } = ColorTranslator.FromHtml("#040404");  // 80%
            public static Color InkBlack { get; private set; } = ColorTranslator.FromHtml("#020202");      // 90%
            public static Color Black { get; private set; } = ColorTranslator.FromHtml("#000000");         // 100%
        }
    }
}

using System.Drawing;

namespace Xpera.Theme
{
  
    public static class Tones
    {
       
        public static class SignalRed
        {
            public static Color MutedRoseRed { get; private set; } = ColorTranslator.FromHtml("#E84661");   // 10%
            public static Color DustyRoseRed { get; private set; } = ColorTranslator.FromHtml("#DD4C65");   // 20%
            public static Color MutedRaspberry { get; private set; } = ColorTranslator.FromHtml("#D15368"); // 30%
            public static Color DustyRose { get; private set; } = ColorTranslator.FromHtml("#C6596C");      // 40%
            public static Color OldRose { get; private set; } = ColorTranslator.FromHtml("#BA606F");        // 50%
            public static Color MutedMauve { get; private set; } = ColorTranslator.FromHtml("#AE6672");     // 60%
            public static Color GrayishRose { get; private set; } = ColorTranslator.FromHtml("#A36D76");    // 70%
            public static Color RoseGray { get; private set; } = ColorTranslator.FromHtml("#977379");       // 80%
            public static Color TaupeGray { get; private set; } = ColorTranslator.FromHtml("#8C7A7D");      // 90%
            public static Color MediumGray { get; private set; } = ColorTranslator.FromHtml("#808080");     // 100%
        }

        public static class Carbon
        {
            public static Color VeryDarkGray { get; private set; } = ColorTranslator.FromHtml("#1D1D1D");  // 10%
            public static Color DarkOnyx { get; private set; } = ColorTranslator.FromHtml("#282828");      // 20%
            public static Color Charcoal { get; private set; } = ColorTranslator.FromHtml("#333333");      // 30%
            public static Color DarkSlateGray { get; private set; } = ColorTranslator.FromHtml("#3E3E3E"); // 40%
            public static Color IronGray { get; private set; } = ColorTranslator.FromHtml("#494949");      // 50%
            public static Color Graphite { get; private set; } = ColorTranslator.FromHtml("#545454");      // 60%
            public static Color SmokeGray { get; private set; } = ColorTranslator.FromHtml("#5F5F5F");     // 70%
            public static Color GraniteGray { get; private set; } = ColorTranslator.FromHtml("#6A6A6A");   // 80%
            public static Color SteelGray { get; private set; } = ColorTranslator.FromHtml("#757575");     // 90%
            public static Color MediumGray { get; private set; } = ColorTranslator.FromHtml("#808080");    // 100%
        }
    }
}

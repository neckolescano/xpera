using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Xpera
{
    public enum Weight { Regular, Medium, SemiBold, Bold }

    // The one place that decides how text looks. Every screen, form, and pop-up takes its fonts from here,
    // so the whole app reads as one product. The font is Inter, the brand's UI font, shipped in the Fonts
    // folder so it works on any PC, even one without Inter installed.
    //
    // The scale (sizes in pixels at 100% Windows scaling; WinForms counts in points, 1 pt = 1.33 px):
    //   Page title 26 px · Section 17 · Card title 15 · Menu 14 · Body and tables 13 · Labels and buttons 13
    //   Supporting text 13 · Small details 12 · Overline (small caps-style labels) 11 · Big numbers 22
    //   Moderately compact for a desktop app used all day: smaller than a website, never below 11 px.
    //   Regular for reading, Medium for labels and buttons, SemiBold for headings and numbers, Bold only for real emphasis.
    public static class Typography
    {
        // Declared first: C# sets these fields from top to bottom, and the tokens below need the cache.
        private static readonly Dictionary<(float, Weight), Font> Cache = new Dictionary<(float, Weight), Font>();
        private static PrivateFontCollection _inter;
        private static bool _loaded;

        public static readonly Font PageTitle = Get(19.5F, Weight.SemiBold);  // 26 px, like "Sales"
        public static readonly Font Section = Get(12.75F, Weight.SemiBold);   // 17 px, like a pop-up title
        public static readonly Font CardTitle = Get(11F, Weight.SemiBold);    // 15 px, like "Customer list"
        public static readonly Font Body = Get(10F, Weight.Regular);          // 13 px, everything you read
        public static readonly Font BodyStrong = Get(10F, Weight.Medium);     // 13 px, a name or value that leads a row
        public static readonly Font Table = Get(10F, Weight.Regular);         // 13 px, table cells
        public static readonly Font TableHeader = Get(9F, Weight.Medium);     // 12 px, column names
        public static readonly Font Label = Get(9.5F, Weight.Medium);         // 13 px, form labels
        public static readonly Font Button = Get(10F, Weight.SemiBold);       // 13 px, button text
        public static readonly Font Nav = Get(10.5F, Weight.Medium);          // 14 px, menu items
        public static readonly Font Support = Get(9.5F, Weight.Regular);      // 13 px, descriptions under titles
        public static readonly Font Meta = Get(9F, Weight.Regular);           // 12 px, dates, counts, hints
        public static readonly Font Tag = Get(8.5F, Weight.Medium);           // 11 px, status tags
        public static readonly Font Overline = Get(8F, Weight.SemiBold);      // 11 px, "TOTAL OWED", "SELL"
        public static readonly Font Number = Get(16.5F, Weight.SemiBold);     // 22 px, tile numbers
        public static readonly Font Money = Get(10.5F, Weight.SemiBold);      // 14 px, a peso amount inside a card

        public static Font Get(float points, Weight weight)
        {
            points = (float)Math.Round(points * 4) / 4;   // quarter points, so the cache stays small
            lock (Cache)
            {
                if (Cache.TryGetValue((points, weight), out Font font)) return font;
                font = Make(points, weight);
                Cache[(points, weight)] = font;
                return font;
            }
        }

        // A token made bigger by the same amount, like the sidebar on a big screen.
        public static Font Scaled(Font token, float scale)
        {
            return Get(token.SizeInPoints * scale, WeightOf(token));
        }

        // Older code and the Designer ask for "size and bold or not". This picks the matching step on the scale,
        // so even those places land on the same few sizes instead of a dozen slightly different ones.
        public static Font FromOld(float size, bool strong)
        {
            if (strong)
            {
                if (size >= 16) return Get(size, Weight.SemiBold);        // big numbers and titles keep their size
                if (size >= 12) return Section;
                if (size >= 10.5F) return CardTitle;
                if (size >= 9.5F) return Button;
                if (size >= 9) return Label;
                if (size >= 8.5F) return Tag;
                return Overline;
            }
            if (size >= 16) return Get(size, Weight.Regular);
            if (size >= 10) return Body;
            if (size >= 9) return Support;
            return Meta;
        }

        public static Weight WeightOf(Font font)
        {
            string name = font.FontFamily.Name;
            if (name.Contains("SemiBold") || name.Contains("Semibold")) return Weight.SemiBold;
            if (name.Contains("Medium")) return Weight.Medium;
            return font.Bold ? Weight.Bold : Weight.Regular;
        }

        public static bool IsInter(Font font)
        {
            return font.FontFamily.Name.StartsWith("Inter");
        }

        private static Font Make(float points, Weight weight)
        {
            Load();
            if (_inter != null)
            {
                string family = weight == Weight.Medium ? "Inter Medium" : weight == Weight.SemiBold ? "Inter SemiBold" : "Inter";
                FontFamily found = _inter.Families.FirstOrDefault(f => f.Name == family);
                if (found != null) return new Font(found, points, weight == Weight.Bold ? FontStyle.Bold : FontStyle.Regular);
            }
            // Fallback: Segoe UI, which every Windows PC has. Medium has no Segoe file, so it uses Semibold.
            bool strong = weight != Weight.Regular;
            return new Font(strong ? "Segoe UI Semibold" : "Segoe UI", points, weight == Weight.Bold ? FontStyle.Bold : FontStyle.Regular);
        }

        // Loads the four Inter files next to the app. GDI+ needs them in a PrivateFontCollection to make fonts,
        // and Windows' own text drawing (labels, text boxes, TextRenderer) needs them registered for this app too.
        private static void Load()
        {
            if (_loaded) return;
            _loaded = true;
            string folder = Path.Combine(AppContext.BaseDirectory, "Fonts");
            string[] files = { "Inter-Regular.ttf", "Inter-Medium.ttf", "Inter-SemiBold.ttf", "Inter-Bold.ttf" };
            if (!files.All(f => File.Exists(Path.Combine(folder, f)))) return;

            PrivateFontCollection collection = new PrivateFontCollection();
            foreach (string file in files)
            {
                string path = Path.Combine(folder, file);
                collection.AddFontFile(path);
                AddFontResourceEx(path, 0x10, IntPtr.Zero);   // 0x10 = FR_PRIVATE, only this app sees it
            }
            _inter = collection;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string path, uint flags, IntPtr reserved);
    }
}

using Xpera.Controls;

namespace Xpera
{
    // Fills a table one page at a time, with search. Every list screen uses one of these.
    // Each row remembers its place in the full list (row.Tag), so edits still reach the right record.
    // The card around the table is as tall as the rows on the page, so a short list makes a short card
    // and the table itself never needs a scrollbar; if the window is too short, the whole page scrolls.
    public class PagedList
    {
        public const int PerPage = Sizes.PerPage;
        private const int MinRows = 3;     // an empty or nearly empty list keeps a little room

        // Runs after any list changes height, so the window can decide if the page needs to scroll.
        public static event Action ContentHeightChanged;

        // For a screen whose height changed another way, like Sales switching to its history tab.
        public static void HeightChanged()
        {
            ContentHeightChanged?.Invoke();
        }

        private readonly DataGridView _grid;
        private readonly Pager _pager;
        private readonly Label _count;
        private readonly string _noun;
        private readonly Func<List<(int Index, object[] Cells)>> _rows;
        private string _query = "";
        private int _page;

        public PagedList(DataGridView grid, Pager pager, Label count, string noun, Func<List<(int Index, object[] Cells)>> rows)
        {
            _grid = grid;
            _pager = pager;
            _count = count;
            _noun = noun;
            _rows = rows;
            _pager.PageChanged += page => { _page = page; Refresh(); };

            // The card sits at the top of the screen at its own height, instead of stretching to the bottom.
            if (_grid.Parent is Card card && card.Dock == DockStyle.Fill) card.Dock = DockStyle.Top;
            _grid.SizeChanged += (sender, e) => { if (!_fitting) FitCard(); };
        }

        // "1 line", "2 lines"; "1 inquiry", "2 inquiries"; "1 entry", "3 entries".
        private string Noun(int count)
        {
            if (count != 1) return _noun;
            if (_noun.EndsWith("ies")) return _noun.Substring(0, _noun.Length - 3) + "y";
            return _noun.EndsWith("s") ? _noun.Substring(0, _noun.Length - 1) : _noun;
        }

        public void Search(string text)
        {
            _query = text.Trim();
            _page = 0;
            Refresh();
        }

        public void Refresh()
        {
            List<(int Index, object[] Cells)> all = _rows();
            List<(int Index, object[] Cells)> found = _query == "" ? all
                : all.Where(r => r.Cells.Any(c => Convert.ToString(c).Contains(_query, StringComparison.OrdinalIgnoreCase))).ToList();

            int pages = Math.Max(1, (found.Count + PerPage - 1) / PerPage);
            _page = Math.Min(_page, pages - 1);
            List<(int Index, object[] Cells)> shown = found.Skip(_page * PerPage).Take(PerPage).ToList();

            _grid.Rows.Clear();
            foreach ((int Index, object[] Cells) row in shown)
            {
                int added = _grid.Rows.Add(row.Cells);
                _grid.Rows[added].Tag = row.Index;
            }
            _grid.CurrentCell = null;

            _count.Text = found.Count == all.Count ? all.Count + " " + Noun(all.Count) : found.Count + " of " + all.Count + " " + Noun(all.Count);
            int from = found.Count == 0 ? 0 : _page * PerPage + 1;
            _pager.Show(_page, pages, from, from + shown.Count - 1, found.Count);
            FitCard();
        }

        private bool _fitting;

        // Makes the card exactly tall enough for the header row, the rows on this page (at least 3),
        // and the sideways scrollbar when the window is too narrow for every column.
        private void FitCard()
        {
            if (!(_grid.Parent is Card card) || card.Dock != DockStyle.Top) return;
            int rows = Math.Max(MinRows, _grid.Rows.Count);
            int want = _grid.ColumnHeadersHeight + rows * _grid.RowTemplate.Height + 2;
            if (_grid.Controls.OfType<HScrollBar>().Any(bar => bar.Visible)) want += SystemInformation.HorizontalScrollBarHeight;
            int change = want - _grid.Height;
            if (change == 0) return;
            _fitting = true;
            card.Height += change;
            _fitting = false;
            ContentHeightChanged?.Invoke();
        }
    }
}

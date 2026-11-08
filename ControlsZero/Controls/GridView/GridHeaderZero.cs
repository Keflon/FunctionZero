using Microsoft.Maui.Layouts;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// Header row for <see cref="GridViewZero"/>. Shows each column's Header (or HeaderTemplate) and a splitter that resizes the column.
/// </summary>
internal sealed class GridHeaderZero : Layout
{
    private const double SplitterWidth = 8.0;
    private readonly GridColumnLayout _columnLayout;
    private readonly List<View> _headers = new();
    private readonly List<GridSplitterZero> _splitters = new();
    private IReadOnlyList<GridColumnZero> _columns = Array.Empty<GridColumnZero>();

    public GridHeaderZero(GridColumnLayout columnLayout)
    {
        _columnLayout = columnLayout;
        IsClippedToBounds = true;
    }

    public event EventHandler ColumnResized;

    public void SetColumns(IReadOnlyList<GridColumnZero> columns)
    {
        _columns = columns;
        Clear();
        _headers.Clear();
        _splitters.Clear();

        foreach (var column in columns)
        {
            View header;
            if (column.HeaderTemplate != null)
            {
                header = (View)column.HeaderTemplate.CreateContent();
                header.BindingContext = column.Header;
            }
            else
            {
                header = new Label { Text = column.Header?.ToString(), VerticalTextAlignment = TextAlignment.Center, Padding = new Thickness(4, 0) };
            }
            _headers.Add(header);
            Add(header);

            var splitter = new GridSplitterZero(column);
            splitter.Resizing += (s, e) => ColumnResized?.Invoke(this, EventArgs.Empty);
            _splitters.Add(splitter);
            Add(splitter);
        }

        InvalidateColumns();
    }

    public void InvalidateColumns()
    {
        for (int c = 0; c < _columns.Count && c < _columnLayout.Widths.Length; c++)
            _splitters[c].CurrentWidth = _columnLayout.Widths[c];

        ((IView)this).InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager() => new HeaderLayoutManager(this);

    private sealed class HeaderLayoutManager : ILayoutManager
    {
        private readonly GridHeaderZero _owner;

        public HeaderLayoutManager(GridHeaderZero owner)
        {
            _owner = owner;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double height = 0;
            var widths = _owner._columnLayout.Widths;
            for (int c = 0; c < _owner._headers.Count && c < widths.Length; c++)
            {
                var size = ((IView)_owner._headers[c]).Measure(widths[c], double.PositiveInfinity);
                height = Math.Max(height, size.Height);
                ((IView)_owner._splitters[c]).Measure(SplitterWidth, double.PositiveInfinity);
            }
            return new Size(double.IsFinite(widthConstraint) ? widthConstraint : _owner._columnLayout.TotalWidth, Math.Max(height, 24));
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var widths = _owner._columnLayout.Widths;
            var offsets = _owner._columnLayout.Offsets;
            for (int c = 0; c < _owner._headers.Count && c < widths.Length; c++)
            {
                ((IView)_owner._headers[c]).Arrange(new Rect(bounds.X + offsets[c], bounds.Y, widths[c], bounds.Height));
                double splitterX = bounds.X + offsets[c] + widths[c] - SplitterWidth / 2;
                ((IView)_owner._splitters[c]).Arrange(new Rect(splitterX, bounds.Y, SplitterWidth, bounds.Height));
            }
            return bounds.Size;
        }
    }
}

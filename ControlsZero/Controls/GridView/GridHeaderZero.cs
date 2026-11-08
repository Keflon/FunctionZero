using Microsoft.Maui.Layouts;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// Header row for <see cref="GridViewZero"/>. Shows each column's Header (or HeaderTemplate) and a splitter that resizes the column.
/// </summary>
internal sealed class GridHeaderZero : Layout
{
    private const double MinimumSplitterWidth = 1.0;
    private readonly GridColumnLayout _columnLayout;
    private readonly List<View> _headers = new();
    private readonly List<GridSplitterZero> _splitters = new();
    private IReadOnlyList<GridColumnZero> _columns = Array.Empty<GridColumnZero>();

    public GridHeaderZero(GridColumnLayout columnLayout)
    {
        _columnLayout = columnLayout;
        IsClippedToBounds = true;
        BackgroundColor = Color.FromRgba(0, 0, 0, 0.01);

        // Gestures live on the header, which never moves, so TotalX is not disturbed by the splitter moving under the pointer.
        var pointer = new PointerGestureRecognizer();
        pointer.PointerPressed += (s, e) => _pressedX = e.GetPosition(this)?.X;
        GestureRecognizers.Add(pointer);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += Pan_PanUpdated;
        GestureRecognizers.Add(pan);
    }

    private const double GrabTolerance = 8.0;
    private double? _pressedX;
    private GridSplitterZero _activeSplitter;

    private void Pan_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _activeSplitter = FindSplitter(_pressedX);
                _activeSplitter?.BeginResize();
                break;
            case GestureStatus.Running:
                _activeSplitter?.Resize(e.TotalX);
                break;
            default:
                _activeSplitter?.EndResize();
                _activeSplitter = null;
                _pressedX = null;
                break;
        }
    }

    /// <summary>Lets the row dividers drive the same resize logic as the header splitters.</summary>
    internal void DragSplitter(int index, GestureStatus status, double totalX)
    {
        if (index < 0 || index >= _splitters.Count)
            return;

        var splitter = _splitters[index];
        switch (status)
        {
            case GestureStatus.Started:
                splitter.BeginResize();
                break;
            case GestureStatus.Running:
                splitter.Resize(totalX);
                break;
            default:
                splitter.EndResize();
                break;
        }
    }

    private GridSplitterZero FindSplitter(double? x)
    {
        if (x == null)
            return null;

        var widths = _columnLayout.Widths;
        var offsets = _columnLayout.Offsets;
        GridSplitterZero best = null;
        double bestDistance = Math.Max(GrabTolerance, _columnLayout.Spacing / 2);
        for (int c = 0; c < _splitters.Count && c < widths.Length; c++)
        {
            double distance = Math.Abs(x.Value - (offsets[c] + widths[c] + _columnLayout.Spacing / 2));
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = _splitters[c];
            }
        }
        return best;
    }

    public event EventHandler ColumnResized;

    private double SplitterWidth => Math.Max(MinimumSplitterWidth, _columnLayout.Spacing);

    private Color _dividerColor = Colors.Gray;

    public Color DividerColor
    {
        get => _dividerColor;
        set
        {
            _dividerColor = value ?? Colors.Gray;
            foreach (var splitter in _splitters)
                splitter.DividerColor = _dividerColor;
        }
    }

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
        }

        // Splitters are added last so they sit above every header and receive the pointer.
        foreach (var column in columns)
        {
            var splitter = new GridSplitterZero(column) { InputTransparent = true, DividerColor = _dividerColor };
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
                ((IView)_owner._splitters[c]).Measure(_owner.SplitterWidth, double.PositiveInfinity);
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
                // The splitter fills the gap after the column; with no gap it straddles the column edge.
                double splitterWidth = _owner.SplitterWidth;
                double splitterX = bounds.X + offsets[c] + widths[c] + (_owner._columnLayout.Spacing - splitterWidth) / 2;
                ((IView)_owner._splitters[c]).Arrange(new Rect(splitterX, bounds.Y, splitterWidth, bounds.Height));
            }
            return bounds.Size;
        }
    }
}

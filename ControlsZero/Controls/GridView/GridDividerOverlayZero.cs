using Microsoft.Maui.Layouts;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// Draws one vertical divider per column gap over the rows area, each with a grip that can be dragged to resize the column.
/// It does not scroll, so it costs nothing during scrolling.
/// </summary>
internal sealed class GridDividerOverlayZero : Layout
{
    private const double MinimumGripWidth = 8.0;
    private readonly GridColumnLayout _columnLayout;
    private readonly List<BoxView> _lines = new();
    private readonly List<ContentView> _grips = new();
    private Color _color = Colors.Gray;
    private int _draggingIndex = -1;

    public GridDividerOverlayZero(GridColumnLayout columnLayout)
    {
        _columnLayout = columnLayout;
        // The overlay lets input through to the rows; only the grips receive input.
        InputTransparent = true;
        CascadeInputTransparent = false;
        IsClippedToBounds = true;
    }

    /// <summary>Raised with the column index, gesture status and total horizontal drag distance.</summary>
    public event Action<int, GestureStatus, double> DividerDragged;

    public Color DividerColor
    {
        get => _color;
        set
        {
            _color = value ?? Colors.Gray;
            foreach (var line in _lines)
                line.Color = _color;
        }
    }

    public void SetCount(int count)
    {
        while (_lines.Count > count)
        {
            int last = _lines.Count - 1;
            Remove(_lines[last]);
            Remove(_grips[last]);
            _lines.RemoveAt(last);
            _grips.RemoveAt(last);
        }
        while (_lines.Count < count)
        {
            int index = _lines.Count;

            var line = new BoxView { Color = _color, InputTransparent = true };
            _lines.Add(line);
            Add(line);

            // Nearly transparent rather than Transparent so the platform hit-tests the whole grip.
            var grip = new ContentView { BackgroundColor = Color.FromRgba(0, 0, 0, 0.01) };
            var pan = new PanGestureRecognizer();
            pan.PanUpdated += (s, e) => Grip_PanUpdated(index, e);
            grip.GestureRecognizers.Add(pan);
            _grips.Add(grip);
            Add(grip);
        }
        Invalidate();
    }

    public void Invalidate() => ((IView)this).InvalidateMeasure();

    private void Grip_PanUpdated(int index, PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
            _draggingIndex = index;

        DividerDragged?.Invoke(index, e.StatusType, e.TotalX);

        if (e.StatusType is GestureStatus.Completed or GestureStatus.Canceled)
        {
            _draggingIndex = -1;
            Invalidate();
        }
    }

    protected override ILayoutManager CreateLayoutManager() => new OverlayLayoutManager(this);

    private sealed class OverlayLayoutManager : ILayoutManager
    {
        private readonly GridDividerOverlayZero _owner;

        public OverlayLayoutManager(GridDividerOverlayZero owner)
        {
            _owner = owner;
        }

        private double GripWidth => Math.Max(MinimumGripWidth, _owner._columnLayout.Spacing);

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double spacing = _owner._columnLayout.Spacing;
            for (int c = 0; c < _owner._lines.Count; c++)
            {
                ((IView)_owner._lines[c]).Measure(spacing, heightConstraint);
                ((IView)_owner._grips[c]).Measure(GripWidth, heightConstraint);
            }

            return new Size(double.IsFinite(widthConstraint) ? widthConstraint : 0, double.IsFinite(heightConstraint) ? heightConstraint : 0);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var layout = _owner._columnLayout;
            var widths = layout.Widths;
            var offsets = layout.Offsets;
            double spacing = layout.Spacing;
            double gripWidth = GripWidth;

            for (int c = 0; c < _owner._lines.Count; c++)
            {
                IView line = _owner._lines[c];
                IView grip = _owner._grips[c];
                bool visible = c < widths.Length && widths[c] > 0 && spacing > 0;
                double x = visible ? bounds.X + offsets[c] + widths[c] : 0;

                line.Arrange(visible ? new Rect(x, bounds.Y, spacing, bounds.Height) : Rect.Zero);

                // The grip being dragged stays put, otherwise TotalX is measured against a moving element.
                if (c != _owner._draggingIndex)
                    grip.Arrange(visible ? new Rect(x + (spacing - gripWidth) / 2, bounds.Y, gripWidth, bounds.Height) : Rect.Zero);
            }
            return bounds.Size;
        }
    }
}

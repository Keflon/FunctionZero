using Microsoft.Maui.Layouts;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// A single realized row. Lays out one <see cref="CellZero"/> per column using the shared column widths.
/// Target this type with a Style (or set GridViewZero.RowStyle) to customise rows.
/// Visual states: Normal, PointerOver, Selected, Primary.
/// </summary>
public class RowZero : Layout
{
    private readonly List<CellZero> _cells = new();

    internal RowZero(GridColumnLayout columnLayout, double rowHeight)
    {
        ColumnLayout = columnLayout;
        RowHeight = rowHeight;

        var tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += (s, e) => Tapped?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tapGestureRecognizer);
    }

    internal event EventHandler Tapped;

    internal GridColumnLayout ColumnLayout { get; }
    internal double RowHeight { get; }
    internal IReadOnlyList<CellZero> Cells => _cells;
    internal object Item { get; set; }
    internal bool IsParked { get; set; }

    public int ItemIndex { get; internal set; } = -1;

    internal void AddCell(CellZero cell)
    {
        _cells.Add(cell);
        Add(cell);
    }

    #region IsSelectedProperty

    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(RowZero), false, BindingMode.OneWay, null, SelectionStateChanged);

    public bool IsSelected
    {
        get { return (bool)GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    #endregion

    #region IsPrimaryProperty

    public static readonly BindableProperty IsPrimaryProperty = BindableProperty.Create(nameof(IsPrimary), typeof(bool), typeof(RowZero), false, BindingMode.OneWay, null, SelectionStateChanged);

    public bool IsPrimary
    {
        get { return (bool)GetValue(IsPrimaryProperty); }
        set { SetValue(IsPrimaryProperty, value); }
    }

    #endregion

    private static void SelectionStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RowZero)bindable).ChangeVisualState();
    }

    protected override void ChangeVisualState()
    {
        if (IsEnabled && IsPrimary)
            VisualStateManager.GoToState(this, "Primary");
        else if (IsEnabled && IsSelected)
            VisualStateManager.GoToState(this, "Selected");
        else
        {
            VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
            base.ChangeVisualState();
        }
    }

    protected override ILayoutManager CreateLayoutManager() => new RowLayoutManager(this);
}

internal sealed class RowLayoutManager : ILayoutManager
{
    private readonly RowZero _row;

    public RowLayoutManager(RowZero row)
    {
        _row = row;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var layout = _row.ColumnLayout;
        var widths = layout.Widths;
        var cells = _row.Cells;
        int count = Math.Min(cells.Count, widths.Length);

        for (int c = 0; c < count; c++)
            ((IView)cells[c]).Measure(widths[c], _row.RowHeight);

        return new Size(Math.Max(layout.TotalWidth, double.IsFinite(widthConstraint) ? widthConstraint : 0), _row.RowHeight);
    }

    public Size ArrangeChildren(Rect bounds)
    {
        var layout = _row.ColumnLayout;
        var widths = layout.Widths;
        var offsets = layout.Offsets;
        var cells = _row.Cells;
        int count = Math.Min(cells.Count, widths.Length);

        for (int c = 0; c < count; c++)
            ((IView)cells[c]).Arrange(new Rect(bounds.X + offsets[c], bounds.Y, widths[c], _row.RowHeight));

        // Return the full slot; WinUI centres a panel that reports a smaller arranged size.
        return new Size(Math.Max(layout.TotalWidth, bounds.Width), _row.RowHeight);
    }
}

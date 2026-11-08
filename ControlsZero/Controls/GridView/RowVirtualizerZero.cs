using FunctionZero.TreeListItemsSourceZero;
using Microsoft.Maui.Layouts;
using System.Collections;
using System.Collections.Specialized;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// The tall scrollable surface of a <see cref="GridViewZero"/>. Owns every realized <see cref="RowZero"/>,
/// keyed by item index, and a single recycle pool.
/// 
/// In 1:1 mode (content height &lt;= MaxScrollHeight) a row's TranslationY is index * RowHeight and is only assigned when the row is realized,
/// so native scrolling moves rows without any per-scroll work and the compositor can never run ahead of the rows.
/// Above MaxScrollHeight the surface is capped and rows are positioned relative to the viewport on each scroll.
/// </summary>
public class RowVirtualizerZero : Layout
{
    public const double MaxScrollHeight = 2000000.0;
    private const int OverscanCount = 8;

    private readonly Dictionary<int, RowZero> _realized = new();
    private readonly Stack<RowZero> _pool = new();
    private readonly List<int> _killList = new();
    private INotifyCollectionChanged _observedItems;
    private IList _items;
    private IReadOnlyList<GridColumnZero> _columns = Array.Empty<GridColumnZero>();
    private double _rowHeight = 40.0;
    private double _viewportHeight;
    private double _scrollY;
    private double _logicalHeight;
    private double _physicalHeight;

    internal RowVirtualizerZero(GridColumnLayout columnLayout)
    {
        ColumnLayout = columnLayout;
        IsClippedToBounds = true;
    }

    internal event EventHandler<RowZero> RowTapped;
    internal event EventHandler<RowZero> RowRealized;

    internal GridColumnLayout ColumnLayout { get; }
    internal IEnumerable<RowZero> RealizedRows => _realized.Values;
    internal Style RowStyle { get; set; }
    internal Func<GridColumnZero, object, object> CellContextSelector { get; set; }

    public double RowHeight => _rowHeight;
    public bool IsScaled => _logicalHeight > MaxScrollHeight;
    public double LogicalHeight => _logicalHeight;
    public double PhysicalHeight => _physicalHeight;

    /// <summary>The index one past the last item in view, or less than ItemsCount if there are more items to come.</summary>
    public int LastVisibleIndex { get; private set; } = -1;

    #region configuration

    internal void SetItems(IList items)
    {
        if (_observedItems != null)
            _observedItems.CollectionChanged -= Items_CollectionChanged;

        _items = items;
        _observedItems = items as INotifyCollectionChanged;

        if (_observedItems != null)
            _observedItems.CollectionChanged += Items_CollectionChanged;

        Reset();
    }

    internal void SetColumns(IReadOnlyList<GridColumnZero> columns)
    {
        _columns = columns;
        // Cell composition changed; nothing can be recycled.
        RecycleAll();
        _pool.Clear();
        Clear();
        Refresh();
    }

    internal void SetRowHeight(double rowHeight)
    {
        if (rowHeight <= 0 || rowHeight == _rowHeight)
            return;

        _rowHeight = rowHeight;
        RecycleAll();
        _pool.Clear();
        Clear();
        Reset();
    }

    internal void SetViewport(double viewportHeight, double scrollY)
    {
        _viewportHeight = viewportHeight;
        _scrollY = scrollY;
        UpdateHeights();
        Refresh();
    }

    internal void Rebind()
    {
        RecycleAll();
        Refresh();
    }

    #endregion

    #region scrolling

    /// <summary>
    /// Maps a native scroll position to a logical offset, in item-space.
    /// </summary>
    internal double ToLogicalOffset(double scrollY)
    {
        if (IsScaled == false)
            return scrollY;

        double physicalRange = Math.Max(1, _physicalHeight - _viewportHeight);
        double logicalRange = Math.Max(0, _logicalHeight - _viewportHeight);
        return Math.Clamp(scrollY / physicalRange, 0, 1) * logicalRange;
    }

    internal double ToPhysicalOffset(double logicalOffset)
    {
        if (IsScaled == false)
            return logicalOffset;

        double logicalRange = Math.Max(1, _logicalHeight - _viewportHeight);
        double physicalRange = Math.Max(0, _physicalHeight - _viewportHeight);
        return Math.Clamp(logicalOffset / logicalRange, 0, 1) * physicalRange;
    }

    internal void OnScrolled(double scrollY)
    {
        _scrollY = scrollY;
        Refresh();
    }

    #endregion

    #region realization

    internal void Refresh()
    {
        if (_items == null || _items.Count == 0 || _columns.Count == 0 || _viewportHeight <= 0)
        {
            RecycleAll();
            LastVisibleIndex = -1;
            return;
        }

        double logicalOffset = ToLogicalOffset(_scrollY);

        int first = Math.Max(0, (int)(logicalOffset / _rowHeight));
        int last = Math.Min(_items.Count - 1, (int)((logicalOffset + _viewportHeight) / _rowHeight));
        LastVisibleIndex = last;

        int firstRealized = Math.Max(0, first - OverscanCount);
        int lastRealized = Math.Min(_items.Count - 1, last + OverscanCount);

        _killList.Clear();
        foreach (var index in _realized.Keys)
            if (index < firstRealized || index > lastRealized)
                _killList.Add(index);

        foreach (var index in _killList)
        {
            var row = _realized[index];
            _realized.Remove(index);
            Park(row);
        }

        for (int index = firstRealized; index <= lastRealized; index++)
        {
            if (_realized.TryGetValue(index, out var row) == false)
            {
                row = Rent();
                _realized[index] = row;
                Bind(row, index);
            }

            if (IsScaled)
                row.TranslationY = _scrollY + index * _rowHeight - logicalOffset;
        }
    }

    private void Bind(RowZero row, int index)
    {
        var item = _items[index];
        row.ItemIndex = index;
        row.Item = item;
        row.IsParked = false;
        row.IsVisible = true;

        var cells = row.Cells;
        for (int c = 0; c < cells.Count; c++)
        {
            var column = _columns[c];
            var cell = cells[c];
            var context = CellContextSelector?.Invoke(column, item) ?? item;

            // Template selection uses the raw item for tree columns (TemplateProvider expects a TreeNodeContainer).
            var template = column.SelectTemplate(column.IsTreeColumn ? item : context);
            if (cell.Template != template)
            {
                cell.BindingContext = null;
                cell.SetTemplate(column, template);
            }
            cell.BindingContext = column.IsTreeColumn ? item : context;
        }

        if (IsScaled == false)
            row.TranslationY = index * _rowHeight;

        RowRealized?.Invoke(this, row);
    }

    private RowZero Rent()
    {
        if (_pool.TryPop(out var row))
            return row;

        row = new RowZero(ColumnLayout, _rowHeight);
        if (RowStyle != null)
            row.Style = RowStyle;
        foreach (var column in _columns)
            row.AddCell(column.CreateCell());
        row.Tapped += (s, e) => RowTapped?.Invoke(this, (RowZero)s);
        Add(row);
        return row;
    }

    /// <summary>
    /// Rows stay in the visual tree when recycled, so realization never adds or removes native views during a scroll.
    /// </summary>
    private void Park(RowZero row)
    {
        row.IsParked = true;
        row.IsVisible = false;
        row.ItemIndex = -1;
        row.Item = null;
        row.IsSelected = false;
        row.IsPrimary = false;
        _pool.Push(row);
    }

    private void RecycleAll()
    {
        foreach (var row in _realized.Values)
            Park(row);
        _realized.Clear();
    }

    internal void ApplyRowStyle(Style style)
    {
        RowStyle = style;
        foreach (var child in Children)
            if (child is RowZero row)
                row.Style = style;
    }

    #endregion

    #region collection changes

    private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewStartingIndex >= 0:
                ShiftFrom(e.NewStartingIndex, e.NewItems.Count);
                break;
            case NotifyCollectionChangedAction.Remove when e.OldStartingIndex >= 0:
                RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                ShiftFrom(e.OldStartingIndex + e.OldItems.Count, -e.OldItems.Count);
                break;
            case NotifyCollectionChangedAction.Replace when e.NewStartingIndex >= 0:
                RemoveRange(e.NewStartingIndex, e.NewItems.Count);
                break;
            case NotifyCollectionChangedAction.Move when e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0:
                int count = e.OldItems.Count;
                RemoveRange(e.OldStartingIndex, count);
                ShiftFrom(e.OldStartingIndex + count, -count);
                ShiftFrom(e.NewStartingIndex, count);
                break;
            default:
                Reset();
                return;
        }

        UpdateHeights();
        Refresh();
    }

    private void RemoveRange(int start, int count)
    {
        for (int index = start; index < start + count; index++)
            if (_realized.Remove(index, out var row))
                Park(row);
    }

    /// <summary>
    /// Re-keys realized rows at or after <paramref name="start"/> by <paramref name="delta"/>, keeping their bound content.
    /// </summary>
    private void ShiftFrom(int start, int delta)
    {
        if (delta == 0)
            return;

        var moved = _realized.Where(kv => kv.Key >= start).OrderBy(kv => delta > 0 ? -kv.Key : kv.Key).ToList();
        foreach (var kv in moved)
            _realized.Remove(kv.Key);

        foreach (var kv in moved)
        {
            int newIndex = kv.Key + delta;
            var row = kv.Value;
            if (newIndex < 0 || _realized.ContainsKey(newIndex))
            {
                Park(row);
                continue;
            }
            row.ItemIndex = newIndex;
            if (IsScaled == false)
                row.TranslationY = newIndex * _rowHeight;
            _realized[newIndex] = row;
        }
    }

    private void Reset()
    {
        RecycleAll();
        UpdateHeights();
        Refresh();
    }

    private void UpdateHeights()
    {
        _logicalHeight = (_items?.Count ?? 0) * _rowHeight;
        double physical = Math.Min(_logicalHeight, MaxScrollHeight);
        if (physical != _physicalHeight)
        {
            _physicalHeight = physical;
            // The surface height drives the native scroll extent.
            HeightRequest = physical;
        }
    }

    #endregion

    protected override ILayoutManager CreateLayoutManager() => new RowVirtualizerLayoutManager(this);

    private sealed class RowVirtualizerLayoutManager : ILayoutManager
    {
        private readonly RowVirtualizerZero _owner;

        public RowVirtualizerLayoutManager(RowVirtualizerZero owner)
        {
            _owner = owner;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double width = Math.Max(_owner.ColumnLayout.TotalWidth, double.IsFinite(widthConstraint) ? widthConstraint : 0);

            foreach (var child in _owner.Children)
                if (child is RowZero row && row.IsParked == false)
                    ((IView)row).Measure(width, _owner._rowHeight);

            return new Size(width, _owner._physicalHeight);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            double width = Math.Max(_owner.ColumnLayout.TotalWidth, bounds.Width);

            // Rows are arranged at the origin and positioned with TranslationY, which is cheap and needs no layout pass.
            foreach (var child in _owner.Children)
                ((IView)child).Arrange(new Rect(bounds.X, bounds.Y, width, _owner._rowHeight));

            return new Size(width, _owner._physicalHeight);
        }
    }
}

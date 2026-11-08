using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// A virtualized, multi-column list. One <see cref="RowZero"/> per realized item hosts a <see cref="CellZero"/> per column.
/// </summary>
[ContentProperty(nameof(Columns))]
public class GridViewZero : ContentView
{
    private readonly GridColumnLayout _columnLayout = new();
    private readonly GridHeaderZero _header;
    private readonly ScrollView _scrollView;
    private readonly RowVirtualizerZero _virtualizer;
    private readonly GridDividerOverlayZero _dividers;
    private readonly List<GridColumnZero> _subscribedColumns = new();
    private bool _isHandlingNativeScroll;
    private bool _updateColumnsRequested;
    private bool _updateSelectionRequested;

    public GridViewZero()
    {
        var columns = new ObservableCollection<GridColumnZero>();
        columns.CollectionChanged += Columns_CollectionChanged;
        Columns = columns;

        _virtualizer = new RowVirtualizerZero(_columnLayout) { RowStyle = DefaultRowStyle, HorizontalOptions = LayoutOptions.Start };
        _virtualizer.RowTapped += Virtualizer_RowTapped;
        _virtualizer.RowRealized += Virtualizer_RowRealized;

        _scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            Content = _virtualizer,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        _scrollView.Scrolled += ScrollView_Scrolled;
        _scrollView.SizeChanged += (s, e) => UpdateViewport();

        _header = new GridHeaderZero(_columnLayout) { IsVisible = false };

        var root = new Grid
        {
            RowSpacing = 0,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        root.Add(_header, 0, 0);
        root.Add(_scrollView, 0, 1);

        _dividers = new GridDividerOverlayZero(_columnLayout);
        _dividers.DividerDragged += (index, status, totalX) => _header.DragSplitter(index, status, totalX);
        root.Add(_dividers, 0, 1);
        Content = root;

        SelectedItems = new List<object>();
    }

    internal static readonly Style DefaultRowStyle = CreateDefaultRowStyle();

    private static Style CreateDefaultRowStyle()
    {
        static VisualState State(string name, Color color)
        {
            var state = new VisualState { Name = name };
            state.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = color });
            return state;
        }

        var group = new VisualStateGroup { Name = "CommonStates" };
        group.States.Add(State("Normal", Colors.Transparent));
        group.States.Add(State("Selected", Colors.AliceBlue));
        group.States.Add(State("Primary", Colors.Cyan));

        var groups = new VisualStateGroupList { group };
        var style = new Style(typeof(RowZero));
        style.Setters.Add(new Setter { Property = VisualStateManager.VisualStateGroupsProperty, Value = groups });
        return style;
    }

    #region Columns

    /// <summary>The column definitions. Declare GridColumnZero elements directly inside the GridViewZero in XAML.</summary>
    public ObservableCollection<GridColumnZero> Columns { get; }

    /// <summary>Alias of <see cref="Columns"/> retained for existing XAML.</summary>
    public IList<GridColumnZero> ColumnsSource => Columns;

    private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_updateColumnsRequested)
            return;

        _updateColumnsRequested = true;
        Dispatcher.Dispatch(UpdateColumns);
    }

    private void UpdateColumns()
    {
        _updateColumnsRequested = false;

        foreach (var column in _subscribedColumns)
        {
            column.LayoutChanged -= Column_LayoutChanged;
            column.HeaderChanged -= Column_HeaderChanged;
            column.TemplateChanged -= Column_TemplateChanged;
            RemoveLogicalChild(column);
        }
        _subscribedColumns.Clear();

        foreach (var column in Columns)
        {
            column.LayoutChanged += Column_LayoutChanged;
            column.HeaderChanged += Column_HeaderChanged;
            column.TemplateChanged += Column_TemplateChanged;
            _subscribedColumns.Add(column);
            // Gives the column an inherited BindingContext and lets {x:Reference} / {Binding Source=...} resolve.
            AddLogicalChild(column);
        }

        _header.SetColumns(_subscribedColumns);
        _dividers.SetCount(_subscribedColumns.Count);
        UpdateColumnLayout(invalidateRows: false);
        _virtualizer.SetColumns(_subscribedColumns);
        UpdateRemainingItems();
    }

    private void Column_HeaderChanged(object sender, EventArgs e) => _header.SetColumns(_subscribedColumns);

    private void Column_LayoutChanged(object sender, EventArgs e)
    {
        UpdateColumnLayout();
    }

    private void Column_TemplateChanged(object sender, EventArgs e) => _virtualizer.SetColumns(_subscribedColumns);

    private void UpdateColumnLayout(bool invalidateRows = true)
    {
        _columnLayout.Compute(_subscribedColumns, _scrollView.Width, Array.Empty<double>());

        // The Windows ScrollView centers content narrower than its viewport, so the rows must always span the viewport.
        double width = Math.Max(_columnLayout.TotalWidth, _scrollView.Width);
        if (width > 0 && _virtualizer.WidthRequest != width)
            _virtualizer.WidthRequest = width;
        _header.InvalidateColumns();
        _dividers.Invalidate();

        if (invalidateRows)
            foreach (var row in _virtualizer.RealizedRows)
                ((IView)row).InvalidateMeasure();

        ((IView)_virtualizer).InvalidateMeasure();
    }

    #endregion

    #region ColumnSpacingProperty

    public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create(nameof(ColumnSpacing), typeof(double), typeof(GridViewZero), 0.0, BindingMode.OneWay, null, ColumnSpacingChanged);

    /// <summary>Gap after each column, in the header and the rows. The gap shows a divider that can be dragged to resize the column.</summary>
    public double ColumnSpacing
    {
        get { return (double)GetValue(ColumnSpacingProperty); }
        set { SetValue(ColumnSpacingProperty, value); }
    }

    private static void ColumnSpacingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
        self._columnLayout.Spacing = Math.Max(0, (double)newValue);
        self.UpdateColumnLayout();
    }

    #endregion

    #region DividerColorProperty

    public static readonly BindableProperty DividerColorProperty = BindableProperty.Create(nameof(DividerColor), typeof(Color), typeof(GridViewZero), Colors.Gray, BindingMode.OneWay, null, DividerColorChanged);

    public Color DividerColor
    {
        get { return (Color)GetValue(DividerColorProperty); }
        set { SetValue(DividerColorProperty, value); }
    }

    private static void DividerColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
        self._header.DividerColor = (Color)newValue;
        self._dividers.DividerColor = (Color)newValue;
    }

    #endregion

    #region ItemsSourceProperty

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(GridViewZero), null, BindingMode.OneWay, null, ItemsSourceChanged);

    public IList ItemsSource
    {
        get { return (IList)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
        self._virtualizer.SetItems((IList)newValue);
        self.UpdateRemainingItems();
    }

    #endregion

    #region RemainingItemsProperty

    public static readonly BindableProperty RemainingItemsProperty = BindableProperty.Create(nameof(RemainingItems), typeof(int), typeof(GridViewZero), -1, BindingMode.OneWay, null, RemainingItemsChanged);

    public int RemainingItems
    {
        get { return (int)GetValue(RemainingItemsProperty); }
        set { SetValue(RemainingItemsProperty, value); }
    }

    private static void RemainingItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
        self.RemainingItemsChangedCommand?.Execute(newValue);
    }

    private void UpdateRemainingItems()
    {
        if (ItemsSource != null && _virtualizer.LastVisibleIndex >= 0)
            RemainingItems = ItemsSource.Count - 1 - _virtualizer.LastVisibleIndex;
    }

    #endregion

    #region RemainingItemsChangedCommand

    public static readonly BindableProperty RemainingItemsChangedCommandProperty = BindableProperty.Create(nameof(RemainingItemsChangedCommand), typeof(ICommand), typeof(GridViewZero), null, BindingMode.OneWay);

    public ICommand RemainingItemsChangedCommand
    {
        get { return (ICommand)GetValue(RemainingItemsChangedCommandProperty); }
        set { SetValue(RemainingItemsChangedCommandProperty, value); }
    }

    #endregion

    #region SelectedItemsProperty

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(IList), typeof(GridViewZero), null, BindingMode.TwoWay, null, SelectedItemsChanged);

    public IList SelectedItems
    {
        get { return (IList)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    private static void SelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= self.SelectedItems_CollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += self.SelectedItems_CollectionChanged;

        self.DeferredSelectionUpdate();
    }

    private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => DeferredSelectionUpdate();

    #endregion

    #region SelectionModeProperty

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(GridViewZero), SelectionMode.None, BindingMode.OneWay, null, (b, o, n) => ((GridViewZero)b).DeferredSelectionUpdate());

    public SelectionMode SelectionMode
    {
        get { return (SelectionMode)GetValue(SelectionModeProperty); }
        set { SetValue(SelectionModeProperty, value); }
    }

    #endregion

    #region SelectedItemProperty

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(GridViewZero), null, BindingMode.TwoWay, null, (b, o, n) => ((GridViewZero)b).DeferredSelectionUpdate());

    public object SelectedItem
    {
        get { return GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    #endregion

    #region ScrollOffsetProperty

    public static readonly BindableProperty ScrollOffsetProperty = BindableProperty.Create(nameof(ScrollOffset), typeof(double), typeof(GridViewZero), 0.0, BindingMode.TwoWay, null, ScrollOffsetChanged);

    /// <summary>Logical scroll offset in item-space, i.e. ItemIndex * ItemHeight.</summary>
    public double ScrollOffset
    {
        get { return (double)GetValue(ScrollOffsetProperty); }
        set { SetValue(ScrollOffsetProperty, value); }
    }

    private static void ScrollOffsetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        // Don't write native scroll movement back to the ScrollView; doing so upsets iOS when scrolling out of bounds.
        if (self._isHandlingNativeScroll)
            return;

        double physical = self._virtualizer.ToPhysicalOffset((double)newValue);
        if (Math.Abs(physical - self._scrollView.ScrollY) >= 0.5)
            self.Dispatcher.Dispatch(async () => await self._scrollView.ScrollToAsync(0, physical, false));
    }

    #endregion

    #region ItemHeightProperty

    public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create(nameof(ItemHeight), typeof(double), typeof(GridViewZero), 40.0, BindingMode.OneWay, null, (b, o, n) => ((GridViewZero)b)._virtualizer.SetRowHeight((double)n));

    public double ItemHeight
    {
        get { return (double)GetValue(ItemHeightProperty); }
        set { SetValue(ItemHeightProperty, value); }
    }

    #endregion

    #region RowStyleProperty

    public static readonly BindableProperty RowStyleProperty = BindableProperty.Create(nameof(RowStyle), typeof(Style), typeof(GridViewZero), null, BindingMode.OneWay, null, (b, o, n) => ((GridViewZero)b)._virtualizer.ApplyRowStyle((Style)n ?? DefaultRowStyle));

    /// <summary>Style applied to every <see cref="RowZero"/>. Use visual states Normal, Selected and Primary.</summary>
    public Style RowStyle
    {
        get { return (Style)GetValue(RowStyleProperty); }
        set { SetValue(RowStyleProperty, value); }
    }

    #endregion

    #region HeaderVisibleProperty

    public static readonly BindableProperty HeaderVisibleProperty = BindableProperty.Create(nameof(HeaderVisible), typeof(bool), typeof(GridViewZero), false, BindingMode.OneWay, null, (b, o, n) => ((GridViewZero)b)._header.IsVisible = (bool)n);

    /// <summary>Shows a header row with column titles and resize splitters.</summary>
    public bool HeaderVisible
    {
        get { return (bool)GetValue(HeaderVisibleProperty); }
        set { SetValue(HeaderVisibleProperty, value); }
    }

    #endregion

    /// <summary>
    /// Maps a raw item to a cell BindingContext for a given column. Used by TreeGridViewZero to present TreeNodeContainer.Data.
    /// </summary>
    internal Func<GridColumnZero, object, object> CellContextSelector
    {
        get => _virtualizer.CellContextSelector;
        set
        {
            _virtualizer.CellContextSelector = value;
            _virtualizer.Rebind();
        }
    }

    private void UpdateViewport()
    {
        if (_scrollView.Width <= 0 || _scrollView.Height <= 0)
            return;

        UpdateColumnLayout();
        _virtualizer.SetViewport(_scrollView.Height, _scrollView.ScrollY);
        UpdateRemainingItems();
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        _isHandlingNativeScroll = true;
        try
        {
            _virtualizer.OnScrolled(e.ScrollY);
            ScrollOffset = _virtualizer.ToLogicalOffset(e.ScrollY);
            UpdateRemainingItems();
        }
        finally
        {
            _isHandlingNativeScroll = false;
        }
    }

    #region selection

    private void Virtualizer_RowRealized(object sender, RowZero row)
    {
        row.IsSelected = SelectedItems?.Contains(row.Item) == true;
        row.IsPrimary = row.Item != null && row.Item == SelectedItem;
    }

    private void Virtualizer_RowTapped(object sender, RowZero row)
    {
        var item = row.Item;
        if (item == null || SelectedItems == null)
            return;

        if (SelectionMode == SelectionMode.None)
            return;

        if (SelectedItems.Contains(item) == false)
        {
            // SelectedItem must be set before SelectedItems changes, so the deferred update keeps it.
            SelectedItem = item;
            SelectedItems.Add(item);
        }
        else
        {
            SelectedItems.Remove(item);
            if (SelectedItem == item)
                SelectedItem = SelectedItems.Count > 0 ? SelectedItems[SelectedItems.Count - 1] : null;
        }

        DeferredSelectionUpdate();
    }

    private void DeferredSelectionUpdate()
    {
        if (_updateSelectionRequested)
            return;

        _updateSelectionRequested = true;
        Dispatcher.Dispatch(() =>
        {
            _updateSelectionRequested = false;

            if (SelectedItems == null)
                return;

            switch (SelectionMode)
            {
                case SelectionMode.None:
                    if (SelectedItems.Count > 0)
                        SelectedItems.Clear();
                    break;
                case SelectionMode.Single:
                    if (SelectedItems.Count > 1)
                    {
                        var last = SelectedItems[SelectedItems.Count - 1];
                        SelectedItems.Clear();
                        SelectedItems.Add(last);
                    }
                    break;
            }

            if (SelectedItem != null && SelectedItems.Contains(SelectedItem) == false)
                SelectedItem = SelectedItems.Count > 0 ? SelectedItems[SelectedItems.Count - 1] : null;

            foreach (var row in _virtualizer.RealizedRows)
            {
                row.IsSelected = SelectedItems.Contains(row.Item);
                row.IsPrimary = row.Item != null && row.Item == SelectedItem;
            }
        });
    }

    #endregion
}

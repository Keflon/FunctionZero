using FunctionZero.Maui.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;

namespace FunctionZero.Maui.Controls;

public partial class GridViewZero : ContentView
{
    private bool _updateItemContainersRequested = false;
    private bool _updateScrollViewContentHeightRequested = false;
    private bool _updateSelectionRequested = false;
    private bool _updateColumnsRequested;
    private const double MAX_SCROLL_HEIGHT = 2000000.0;
    private double _scaleToControl = 1.0;

    protected override Size ArrangeOverride(Rect bounds)
    {
        // This is here so the GridColumnZero instances initially render even if their ItemsSource is already set.
        DeferredUpdateScrollViewContentHeight();
        DeferredUpdateItemContainers();

        return base.ArrangeOverride(bounds);

    }

    internal CustomScrollView TheScrollView => theScrollView;
    private static int _instanceIndex = 0;
    private int _thisIndex;

    public GridViewZero()
    {
        var columns = new ObservableCollection<GridColumnZero>();
        columns.CollectionChanged += ColumnsSource_CollectionChanged;
        ColumnsSource = columns;

        InitializeComponent();
        TheScrollView.Scrolled += ScrollView_Scrolled;
        theGrid.MeasureInvalidated += TheGrid_MeasureInvalidated;
        _instanceIndex++;
        _thisIndex = _instanceIndex;

    }

    private void TheGrid_MeasureInvalidated(object? sender, EventArgs e)
    {
        //UpdateItemContainers();
        DeferredUpdateItemContainers();
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        ScrollOffset = e.ScrollY / _scaleToControl;
    }

    #region bindable properties

    #region ColumnsSourceProperty

    public static readonly BindableProperty ColumnsSourceProperty = BindableProperty.Create(nameof(ColumnsSource), typeof(IList<GridColumnZero>), typeof(GridViewZero), null, BindingMode.OneWay, null, ColumnsSourceChanged);

    public IList<GridColumnZero> ColumnsSource
    {
        get { return (IList<GridColumnZero>)GetValue(ColumnsSourceProperty); }
        set { SetValue(ColumnsSourceProperty, value); }
    }

    private static void ColumnsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= self.ColumnsSource_CollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += self.ColumnsSource_CollectionChanged;

        self.RequestUpdateColumns();
        //self.DeferredUpdateScrollViewContentHeight();
        //self.DeferredUpdateItemContainers();
    }

    private void ColumnsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RequestUpdateColumns();
    }
    private void RequestUpdateColumns()
    {
        if (_updateColumnsRequested == false)
        {
            _updateColumnsRequested = true;
            Dispatcher.Dispatch(() => DeferredUpdateColumns());
        }
    }

    private void DeferredUpdateColumns()
    {
        // TODO: Force TheGrid to reflect ColumnsSource.
        // TODO: For now, just brute-force it.

        theGrid.ColumnDefinitions.Clear();

        var columnKillList = new List<GridColumnZero>();
        var splitterKillList = new List<GridSplitterZero>();
        foreach (var item in theGrid.Children)
            if (item is GridColumnZero gcz)
                columnKillList.Add(gcz);
            else if (item is GridSplitterZero gsz)
                splitterKillList.Add(gsz);

        foreach (var item in splitterKillList)
            theGrid.Children.Remove(item);

        foreach (var item in columnKillList)
        {
            item.ItemTapped -= Item_ItemTapped;
            theGrid.Children.Remove(item);
        }

        //theGrid.Children.Clear();

        int index = 0;

        foreach (var columnZero in ColumnsSource)
        {
            Debug.WriteLine($"Instance Count:{_instanceIndex}, Instance Index:{_thisIndex}, Column Index: {index}");
            if (index != 0)
            {
                theGrid.ColumnDefinitions.Add(new ColumnDefinition(35));
                var splitter = new GridSplitterZero() { BackgroundColor = Colors.Purple };
                theGrid.Children.Insert(0, splitter);
                splitter.SetValue(Grid.ColumnProperty, index);
                index++;
            }
            theGrid.ColumnDefinitions.Add(new ColumnDefinition(250));

            columnZero.ItemTapped += Item_ItemTapped;

            theGrid.Children.Insert(0, columnZero);
            columnZero.SetValue(Grid.ColumnProperty, index);
            index++;
        }
        _updateColumnsRequested = false;
    }

    private void Item_ItemTapped(object? sender, ListItemTappedEventArgs e)
    {
        // A ListItemZero cell has been tapped by the user.

        if (e.TappedItem.BindingContext != null)
        {
            if (SelectionMode != SelectionMode.None)
            {
                if (e.TappedItem.IsSelected == false)
                {
                    // Adding to SelectedItems will cause a deferred update.
                    // SelectedItem must be set prior to that call.
                    SelectedItem = e.TappedItem.BindingContext;
                    SelectedItems.Add(e.TappedItem.BindingContext);
                }
                else
                {
                    SelectedItems.Remove(e.TappedItem.BindingContext);
                    if (SelectedItem == e.TappedItem.BindingContext)
                    {
                        if (SelectedItems?.Count > 0)
                            SelectedItem = SelectedItems[SelectedItems.Count - 1];
                        else
                            SelectedItem = null;
                    }
                }
            }
            else
                e.TappedItem.IsSelected = false;

            DeferredSelectionUpdate();

        }
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

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= self.ItemsSource_CollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += self.ItemsSource_CollectionChanged;

        self.DeferredUpdateScrollViewContentHeight();
        self.DeferredUpdateItemContainers();
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

    #endregion

    #region RemainingItemsChangedCommand

    public static readonly BindableProperty RemainingItemsChangedCommandProperty = BindableProperty.Create(nameof(RemainingItemsChangedCommand), typeof(ICommand), typeof(GridViewZero), null, BindingMode.OneWay, null, RemainingItemsChangedCommandChanged);

    public ICommand RemainingItemsChangedCommand
    {
        get { return (ICommand)GetValue(RemainingItemsChangedCommandProperty); }
        set { SetValue(RemainingItemsChangedCommandProperty, value); }
    }

    private static void RemainingItemsChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
    }

    #endregion

    #region SelectedItemsProperty

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(IList), typeof(GridViewZero), new List<object>(), BindingMode.TwoWay, null, SelectedItemsChanged);

    private static void SelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= self.SelectedItems_CollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += self.SelectedItems_CollectionChanged;

        // This will bail early if the change was caused by SelectionUpdate.
        self.DeferredSelectionUpdate();
    }
    public IList SelectedItems
    {
        get { return (IList)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    #endregion

    #region SelectionModeProperty

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(GridViewZero), SelectionMode.None, BindingMode.OneWay, null, SelectionModeChanged);

    public SelectionMode SelectionMode
    {
        get { return (SelectionMode)GetValue(SelectionModeProperty); }
        set { SetValue(SelectionModeProperty, value); }
    }

    private static void SelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        self.DeferredSelectionUpdate();
    }

    #endregion

    #region SelectedItemProperty

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(GridViewZero), null, BindingMode.TwoWay, null, SelectedItemChanged);

    private static void SelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
    }
    public object SelectedItem
    {
        get { return (object)GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    #endregion

    #region ScrollOffsetProperty

    public static readonly BindableProperty ScrollOffsetProperty = BindableProperty.Create(nameof(ScrollOffset), typeof(double), typeof(GridViewZero), (double)0.0, BindingMode.TwoWay, null, ScrollOffsetChanged, null);

    private static void ScrollOffsetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        // TODO: Lookahead and Lookbehind values; to prevent flicker on Windows when fast scolling.

        // Windows is less flickery if we defer the update here. That doesn't make sense!
        // iOS and Droid don't flicker and look the same either way. That doesn't make sense either!
        self.DeferredUpdateItemContainers();

        // If the scroll movement came from the ScrollView don't write it back, because doing so upsets iOS when scrolling outside of bounds and is unnecessary anyway.

        double actualScrollOffset = self.theScrollView.ScrollY / self._scaleToControl;

        if (self.ScrollOffset != actualScrollOffset)
            //self.DeferredScrollTo(self.ScrollOffset * self._scaleToControl);
            self.theScrollView.ScrollYRequest = self.ScrollOffset * self._scaleToControl;
    }
    public double ScrollOffset
    {
        get { return (double)GetValue(ScrollOffsetProperty); }
        set { SetValue(ScrollOffsetProperty, value); }
    }

    #endregion

    #region ItemHeightProperty

    public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create(nameof(ItemHeight), typeof(double), typeof(GridViewZero), (double)40.0, BindingMode.OneWay);

    public double ItemHeight
    {
        get { return (double)GetValue(ItemHeightProperty); }
        set { SetValue(ItemHeightProperty, value); }
    }

    #endregion

    #region ItemContainerStyleProperty

    public static readonly BindableProperty ItemContainerStyleProperty = BindableProperty.Create(nameof(ItemContainerStyle), typeof(Style), typeof(GridViewZero), null, BindingMode.OneWay, null, ItemContainerStyleChanged);

    public Style ItemContainerStyle
    {
        get { return (Style)GetValue(ItemContainerStyleProperty); }
        set { SetValue(ItemContainerStyleProperty, value); }
    }

    private static void ItemContainerStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;
    }

    #endregion

    #endregion


    private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // This will bail early if the change was caused by DeferredFilter.
        DeferredSelectionUpdate();
    }

    private void DeferredSelectionUpdate()
    {
        if (_updateSelectionRequested == false)
        {
            _updateSelectionRequested = true;
            // The underlying collection can have items added / removed in a foreach,
            // and this buffers that down to 1 operation.
            Dispatcher.Dispatch(() =>
            {
                switch (SelectionMode)
                {
                    case SelectionMode.None:
                        SelectedItems.Clear();
                        break;
                    case SelectionMode.Single:
                        if (SelectedItems.Count > 1)
                        {
                            // TODO: KillList, to reduce callbacks if SelectedItems is observable.
                            var temp = SelectedItems[SelectedItems.Count - 1];
                            SelectedItems.Clear();
                            SelectedItems.Add(temp);
                        }
                        break;
                    case SelectionMode.Multiple:
                        break;
                }

                if (SelectedItems.Contains(SelectedItem) == false)
                {
                    if (SelectedItems.Count > 0)
                        SelectedItem = SelectedItems[SelectedItems.Count - 1];
                    else
                        SelectedItem = null;
                }

                //foreach (View item in this.theScrollView.Canvas)
                //    if (item is ListItemZero listItem)
                //    {
                //        listItem.IsSelected = SelectedItems.Contains(listItem.BindingContext);
                //        listItem.IsPrimary = listItem.BindingContext == SelectedItem;
                //    }

                foreach (var column in this.ColumnsSource)
                {
                    foreach (View item in column.Canvas)
                    {
                        if (item is ListItemZero listItem)
                        {
                            listItem.IsSelected = SelectedItems.Contains(listItem.BindingContext);
                            listItem.IsPrimary = listItem.BindingContext == SelectedItem;
                        }
                    }
                }

                _updateSelectionRequested = false;
            }
            );
        }
    }

    private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        DeferredUpdateScrollViewContentHeight();
        DeferredUpdateItemContainers();
    }

    private void DeferredUpdateScrollViewContentHeight()
    {
        if (_updateScrollViewContentHeightRequested == false)
        {
            _updateScrollViewContentHeightRequested = true;

            Dispatcher.Dispatch(() =>
            {
                UpdateScrollViewContentHeight();

#if false
// TODO: If anyone needs it, cap the HeightRequest to something manageable and scale the rendering offsets appropriately.

                //canvas.HeightRequest = 2090000;
                //canvas.HeightRequest = 2100000;
                //canvas.HeightRequest = 2097590;clip region slightly too big
                //canvas.HeightRequest = 2098600; //clip region massive/ not working 
                //canvas.HeightRequest = 2097589; 
#endif
                _updateScrollViewContentHeightRequested = false;
            }
            );
        }
    }

    private void UpdateScrollViewContentHeight()
    {
        double desiredHeight = ItemHeight * ItemsSource?.Count ?? 0;

        if (desiredHeight > MAX_SCROLL_HEIGHT)
        {
            _scaleToControl = MAX_SCROLL_HEIGHT / desiredHeight;
            theScrollView.ContentHeight = MAX_SCROLL_HEIGHT + (this.Height - this.Height * (_scaleToControl));
        }
        else
        {
            theScrollView.ContentHeight = desiredHeight;
            _scaleToControl = 1.0;
        }
    }

    private void DeferredUpdateItemContainers()
    {
        if (_updateItemContainersRequested == false)
        {
            _updateItemContainersRequested = true;
            // The underlying collection can have items added / removed in a foreach,
            // and this buffers that down to 1 call to UpdateItemContainers.
            Dispatcher.Dispatch(() =>
            {
                UpdateItemContainers();
                _updateItemContainersRequested = false;
            }
            );
        }
    }

    private void UpdateItemContainers()
    {
        BatchBegin();

        foreach (var item in ColumnsSource)
        {
            item.UpdateItemContainers(this);
        }

        BatchCommit();
    }
}
using FunctionZero.Maui.Controls;
using FunctionZero.Maui.Services.Cache;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace FunctionZero.Maui.Controls;

public partial class GridColumnZero : ContentView
{
    private readonly BucketDictionary<DataTemplate, ListItemZero> _cache;
    private readonly List<ListItemZero> _killList;
    private bool _pendingUpdateItemContainers = false;
    private bool _pendingSelectionUpdate = false;
    private bool _pendingScrollUpdate = false;
    private bool _updatingItemContainers = false;
    private bool _pendingUpdateScrollViewContentHeight = false;
    private double _scaleToControl = 1.0;

    private const double MAX_SCROLL_HEIGHT = 2000000.0;


    #region bindable properties

    #region ItemTemplateProperty

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(GridColumnZero), null, BindingMode.OneWay);

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    #endregion


    #region ItemContainerStyleProperty

    public static readonly BindableProperty ItemContainerStyleProperty = BindableProperty.Create(nameof(ItemContainerStyle), typeof(Style), typeof(GridColumnZero), null, BindingMode.OneWay, null, ItemContainerStyleChanged);

    public Style ItemContainerStyle
    {
        get { return (Style)GetValue(ItemContainerStyleProperty); }
        set { SetValue(ItemContainerStyleProperty, value); }
    }

    private static void ItemContainerStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridColumnZero)bindable;
    }

    #endregion

    #endregion


    public GridColumnZero()
    {
        _cache = new();
        _killList = new(50);

        InitializeComponent();
    }

    internal void UpdateItemContainers(GridViewZero owner)
    {
        if (owner.TheScrollView.Canvas.Height <= 0)
            return;

        if (owner.ItemsSource == null)
            return;
         
        if (_updatingItemContainers == true)
        {
            Debug.WriteLine("Gotcha!");
            //return;
        }

        _updatingItemContainers = true;

        var adjustedScrollOffset = owner.ScrollOffset;// / _scaleToControl;
        //var adjustedScrollOffset = scrollView.ScrollY;

        // Find the first item that is to be in view
        int firstVisibleIndex = Math.Max(0, (int)(adjustedScrollOffset / owner.ItemHeight));

        // Maximum number of ListItem instances that can be at least partially seen.
        int maxVisibleContainers = (int)(owner.TheScrollView.Height / owner.ItemHeight) + 1;

        //Debug.WriteLine($"ScrollOffset: {ScrollOffset}, scrollView.ScrollY: {scrollView.ScrollY}, scrollView.ScrollY / _scaleToControl: {scrollView.ScrollY / _scaleToControl}, ScrollOffset * _scaleToControl: {ScrollOffset * _scaleToControl}");

        int lastVisibleIndex = Math.Min(owner.ItemsSource.Count - 1, firstVisibleIndex + maxVisibleContainers);

        int lastVisibleCellIndex = firstVisibleIndex + maxVisibleContainers;
        //RemainingItems = ItemsSource.Count - 1 - lastVisibleIndex;

        // RemainingItems can go -ve if there are not enough items to fill the view.
        owner.RemainingItems = owner.ItemsSource.Count - lastVisibleCellIndex;

        //Debug.WriteLine($"RI:{RemainingItems}, from:{ItemsSource.Count - 1 - lastVisibleIndex}");

        // Foreach over each ListItemZero in this.Canvas and set ItemIndex to -1.
        // Then, after layout, kill any that still have ItemIndex of -1.
        // Mark everything in the canvas as a candidate for removal.
        foreach (View item in TheGridColumnCanvas)
            if (item is ListItemZero listItem)
                listItem.ItemIndex = -1;

        // For each item that should be visible,
        // get an existing ListViewItem from the canvas
        // if there isn't one, get one from the cache
        // if there isn't one, create one and set it up.
        for (int c = firstVisibleIndex; c <= lastVisibleIndex; c++)
        {
            ListItemZero listItem = GetViewForBindingContextFromCanvas(owner.ItemsSource[c]);
            if (listItem == null)
            {
                listItem = GetView(owner.ItemsSource[c]);
                listItem.HeightRequest = owner.ItemHeight;
                //listItem is either newly created or retrieved from the cache.
                // SMELL: BindingContext should already be unset, so no need to null. The TreeNodeSpacer needs this for some reason. Fix the TreeNodeSpacer.
                listItem.BindingContext = null;
                // set IsSelected *before* it gets a BindingContext, i.e. before adding to the canvas.
                listItem.IsSelected = owner.SelectedItems.Contains(owner.ItemsSource[c]);
                listItem.IsPrimary = owner.ItemsSource[c] == owner.SelectedItem;

                // SMELL: canvas will provide a BC, so we should set BC first. The TreeNodeSpacer needs this for some reason. Fix the TreeNodeSpacer.
                this.TheGridColumnCanvas.Add(listItem);
                listItem.BindingContext = owner.ItemsSource[c];
            }
            listItem.ItemIndex = c;
        }

        _killList.Clear();

        foreach (View item in this.TheGridColumnCanvas)
            if (item is ListItemZero listItem)
                if (listItem.ItemIndex == -1)
                    _killList.Add(listItem);

        foreach (ListItemZero item in _killList)
        {
            //item.BindingContext = null;
            this.TheGridColumnCanvas.Remove(item);
            item.ClearValue(ListItemZero.BindingContextProperty);
            item.IsSelected = false;
            item.IsPrimary = false;
            _cache.PushToBucket(item.ItemTemplate, item);
        }

        foreach (object item in this.TheGridColumnCanvas)
        {
            if (item is ListItemZero listItem)
            {
                // Determine offset for item.
                double itemOffset = listItem.ItemIndex * owner.ItemHeight - adjustedScrollOffset;
                //double itemOffset = listItem.ItemIndex * ItemHeight - scrollView.ScrollY;
                listItem.BindingContext = owner.ItemsSource[listItem.ItemIndex];
                listItem.TranslationY = itemOffset;
                listItem.WidthRequest = this.Width;
            }
        }
        _updatingItemContainers = false;
    }

    private ListItemZero GetViewForBindingContextFromCanvas(object bindingContext)
    {
        // TODO: Will a Map be quicker? Probably not.
        foreach (View item in this.TheGridColumnCanvas)
            if (item.BindingContext == bindingContext)
                return (ListItemZero)item;

        return null;
    }

    private ListItemZero GetView(object bindingContext)
    {
        ListItemZero retVal = null;

        DataTemplate template;

        if (ItemTemplate is DataTemplateSelector selector)
            template = selector.SelectTemplate(bindingContext, null);
        else
            template = ItemTemplate;

        if (_cache.TryPopFromBucket(template, out var cachedThing, bindingContext))
            retVal = cachedThing;

        if (retVal == null)
        {
            retVal = new ListItemZero();

            retVal.ItemTemplate = template;
            retVal.Content = (View)template.CreateContent();

            if (ItemContainerStyle != null)
                retVal.Style = ItemContainerStyle;

            // No need to unsubscribe, because this object is cached for re-use rather than disposed.
            retVal.PropertyChanged += ListItemZero_PropertyChanged;
        }

        //retVal.HeightRequest = ItemHeight;
        return retVal;
    }

    private void ListItemZero_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ListItemZero.IsSelected))
        {
            var listItem = (ListItemZero)sender;

            throw new NotImplementedException();

            //if (listItem.BindingContext != null)
            //{
            //    if (SelectionMode != SelectionMode.None)
            //    {
            //        if (listItem.IsSelected)
            //        {
            //            // Adding to SelectedItems will cause a deferred update.
            //            // SelectedItem must be set prior to that call.
            //            SelectedItem = listItem.BindingContext;
            //            SelectedItems.Add(listItem.BindingContext);
            //        }
            //        else
            //        {
            //            SelectedItems.Remove(listItem.BindingContext);
            //            if (SelectedItem == listItem.BindingContext)
            //            {
            //                if (SelectedItems?.Count > 0)
            //                    SelectedItem = SelectedItems[SelectedItems.Count - 1];
            //                else
            //                    SelectedItem = null;
            //            }
            //        }
            //    }
            //    else
            //        listItem.IsSelected = false;
            //}
        }
    }
}

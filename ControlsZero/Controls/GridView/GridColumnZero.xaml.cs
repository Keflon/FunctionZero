using FunctionZero.Maui.Services.Cache;
using System.ComponentModel;
using System.Diagnostics;

namespace FunctionZero.Maui.Controls;

public partial class GridColumnZero : ContentView
{
    private readonly BucketDictionary<DataTemplate, ListItemZero> _cache;
    private readonly List<ListItemZero> _killList;
    private bool _updatingItemContainers = false;

    public event EventHandler<ListItemTappedEventArgs> ItemTapped;
    public Layout Canvas => TheGridColumnCanvas;

    public bool IsPartOfTree_controlTemplateBugWorkaround { get; set; }

    #region bindable properties

    #region ItemTemplateProperty

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(GridColumnZero), GetDefaultTemplate(), BindingMode.OneWay, null, ItemTemplatePropertyChanged);

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    private static void ItemTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {

    }

    private static DataTemplate GetDefaultTemplate()
    {
        var template = new DataTemplate(() =>
        {
            var retval = new Label();
            retval.SetBinding(Label.TextProperty, ".");
            return retval;
        });
        return template;
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
            //retVal.PropertyChanged += ListItemZero_PropertyChanged;
            retVal.ItemTapped += RetVal_ItemTapped;
        }
        return retVal;
    }

    private void RetVal_ItemTapped(object? sender, ListItemTappedEventArgs e)
    {
        // No need for null check, because if there is no subscriber, we have a problem!
        ItemTapped(this, e);
    }
}

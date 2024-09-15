using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FunctionZero.Maui.Controls;

public partial class ListViewZero : ContentView
{
    #region bindable properties

    #region ItemsSourceProperty

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(ListViewZero), null, BindingMode.OneWay, null, ItemsSourceChanged);

    public IList ItemsSource
    {
        get { return (IList)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }

    #endregion

    #region RemainingItemsProperty

    public static readonly BindableProperty RemainingItemsProperty = BindableProperty.Create(nameof(RemainingItems), typeof(int), typeof(ListViewZero), -1, BindingMode.OneWay, null, RemainingItemsChanged);

    public int RemainingItems
    {
        get { return (int)GetValue(RemainingItemsProperty); }
        set { SetValue(RemainingItemsProperty, value); }
    }

    private static void RemainingItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;

        self.RemainingItemsChangedCommand?.Execute(newValue);
    }

    #endregion

    #region RemainingItemsChangedCommand

    public static readonly BindableProperty RemainingItemsChangedCommandProperty = BindableProperty.Create(nameof(RemainingItemsChangedCommand), typeof(ICommand), typeof(ListViewZero), null, BindingMode.OneWay, null, RemainingItemsChangedCommandChanged);

    public ICommand RemainingItemsChangedCommand
    {
        get { return (ICommand)GetValue(RemainingItemsChangedCommandProperty); }
        set { SetValue(RemainingItemsChangedCommandProperty, value); }
    }

    private static void RemainingItemsChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }

    #endregion

    #region SelectedItemsProperty

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(nameof(SelectedItems), typeof(IList), typeof(ListViewZero), null, BindingMode.TwoWay, null, SelectedItemsChanged);

    private static void SelectedItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }
    public IList SelectedItems
    {
        get { return (IList)GetValue(SelectedItemsProperty); }
        set { SetValue(SelectedItemsProperty, value); }
    }

    #endregion

    #region SelectionModeProperty

    public static readonly BindableProperty SelectionModeProperty = BindableProperty.Create(nameof(SelectionMode), typeof(SelectionMode), typeof(ListViewZero), SelectionMode.None, BindingMode.OneWay, null, SelectionModeChanged);

    public SelectionMode SelectionMode
    {
        get { return (SelectionMode)GetValue(SelectionModeProperty); }
        set { SetValue(SelectionModeProperty, value); }
    }

    private static void SelectionModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }

    #endregion

    #region SelectedItemProperty

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ListViewZero), null, BindingMode.TwoWay, null, SelectedItemChanged);

    private static void SelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }
    public object SelectedItem
    {
        get { return (object)GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    #endregion

    #region ItemTemplateProperty

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ListViewZero), null, BindingMode.OneWay);

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    #endregion

    #region ScrollOffsetProperty

    public static readonly BindableProperty ScrollOffsetProperty = BindableProperty.Create(nameof(ScrollOffset), typeof(double), typeof(ListViewZero), (double)0.0, BindingMode.TwoWay, null, ScrollOffsetChanged, null/*, CoerceScrollOffsetValue*/);

    private static void ScrollOffsetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }
    public double ScrollOffset
    {
        get { return (double)GetValue(ScrollOffsetProperty); }
        set { SetValue(ScrollOffsetProperty, value); }
    }

    #endregion

    public static readonly BindableProperty ItemHeightProperty = BindableProperty.Create(nameof(ItemHeight), typeof(double), typeof(ListViewZero), (double)40.0, BindingMode.OneWay);

    public double ItemHeight
    {
        get { return (double)GetValue(ItemHeightProperty); }
        set { SetValue(ItemHeightProperty, value); }
    }

    public static readonly BindableProperty ItemContainerStyleProperty = BindableProperty.Create(nameof(ItemContainerStyle), typeof(Style), typeof(ListViewZero), null, BindingMode.OneWay, null, ItemContainerStyleChanged);

    public Style ItemContainerStyle
    {
        get { return (Style)GetValue(ItemContainerStyleProperty); }
        set { SetValue(ItemContainerStyleProperty, value); }
    }

    private static void ItemContainerStyleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (ListViewZero)bindable;
    }

    #endregion

   
    public ListViewZero()
    {
        SelectedItems = new ObservableCollection<object>();
        InitializeComponent();
    }
}

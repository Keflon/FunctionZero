using FunctionZero.Maui.Controls;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FunctionZero.Maui.Controls;

public partial class GridViewZero : ContentView
{
	public GridViewZero()
	{
		InitializeComponent();
	}

    #region ColumnsSourceProperty

    public static readonly BindableProperty ColumnsSourceProperty = BindableProperty.Create(nameof(ColumnsSource), typeof(ObservableCollection<GridColumnZero>), typeof(GridViewZero), null, BindingMode.OneWay, null, ColumnsSourceChanged);

    public ObservableCollection<GridColumnZero> ColumnsSource
    {
        get { return (ObservableCollection<GridColumnZero>)GetValue(ColumnsSourceProperty); }
        set { SetValue(ColumnsSourceProperty, value); }
    }

    private static void ColumnsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridViewZero)bindable;

        if (oldValue is INotifyCollectionChanged oldCollection)
            oldCollection.CollectionChanged -= self.ColumnsSource_CollectionChanged;

        if (newValue is INotifyCollectionChanged newCollection)
            newCollection.CollectionChanged += self.ColumnsSource_CollectionChanged; 

        self.UpdateColumns();
        //self.DeferredUpdateScrollViewContentHeight();
        //self.DeferredUpdateItemContainers();
    }

    private void ColumnsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateColumns();
    }

    private void UpdateColumns()
    {
        // TODO: Force TheGrid to reflect ColumnsSource.
        // TODO: For now, just brute-force it.

        theGrid.ColumnDefinitions.Clear();
        theGrid.Children.Clear();

        int index = 0;

        foreach (var item in ColumnsSource)
        {
            if(index != 0)
            {
                theGrid.ColumnDefinitions.Add(new ColumnDefinition(35));
                var splitter = new GridSplitterZero() { BackgroundColor = Colors.Purple };
                theGrid.Children.Add(splitter);
                splitter.SetValue(Grid.ColumnProperty, index);
                index++;
            }
            theGrid.ColumnDefinitions.Add(new ColumnDefinition(250));
            theGrid.Children.Add(item);
            item.SetValue(Grid.ColumnProperty, index);
            index++;
        }
    }

    #endregion
}
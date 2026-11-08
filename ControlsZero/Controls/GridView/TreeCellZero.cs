using FunctionZero.TreeListItemsSourceZero;

namespace FunctionZero.Maui.Controls;

/// <summary>
/// A cell for the tree column. Draws an indent and a <see cref="Chevron"/> bound to the <see cref="TreeNodeContainer{T}"/>,
/// and presents the column template with the node's Data as its BindingContext.
/// </summary>
public class TreeCellZero : CellZero
{
    private readonly GridColumnZero _column;
    private readonly BoxView _spacer;
    private readonly ContentView _presenter;

    internal TreeCellZero(GridColumnZero column)
    {
        _column = column;

        _spacer = new BoxView { Color = Colors.Transparent, WidthRequest = 0, InputTransparent = true };

        var chevron = new Chevron();
        chevron.SetBinding(Chevron.IsExpandedProperty, new Binding(nameof(TreeNodeContainer<object>.IsExpanded), BindingMode.TwoWay));
        chevron.SetBinding(Chevron.ShowChevronProperty, new Binding(nameof(TreeNodeContainer<object>.ShowChevron), BindingMode.OneWay));

        // Setting the BindingContext explicitly stops it inheriting the TreeNodeContainer, so the content only ever sees the Data.
        _presenter = new ContentView { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, BindingContext = null };

        var grid = new Grid
        {
            ColumnSpacing = 0,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };
        grid.Add(_spacer, 0, 0);
        grid.Add(chevron, 1, 0);
        grid.Add(_presenter, 2, 0);

        Content = grid;
    }

    protected override View PresentedContent
    {
        get => _presenter.Content;
        set => _presenter.Content = value;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is TreeNodeContainer<object> node)
        {
            _spacer.WidthRequest = Math.Max(0, _column.IndentMultiplier * (node.Indent - 1));
            _presenter.BindingContext = node.Data;
        }
        else
        {
            _presenter.BindingContext = null;
        }
    }
}

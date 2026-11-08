namespace FunctionZero.Maui.Controls;

/// <summary>
/// Describes a column of a <see cref="GridViewZero"/>. Not a visual; the grid creates one <see cref="CellZero"/> per realized row.
/// </summary>
public class GridColumnZero : Element
{
    private readonly Dictionary<DataTemplate, Stack<View>> _contentCache = new();

    internal event EventHandler LayoutChanged;
    internal event EventHandler TemplateChanged;

    #region ItemTemplateProperty

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(GridColumnZero), DefaultTemplate, BindingMode.OneWay, null, OnTemplateChanged);

    public DataTemplate ItemTemplate
    {
        get { return (DataTemplate)GetValue(ItemTemplateProperty); }
        set { SetValue(ItemTemplateProperty, value); }
    }

    internal static readonly DataTemplate DefaultTemplate = new DataTemplate(() =>
    {
        var label = new Label { VerticalTextAlignment = TextAlignment.Center };
        label.SetBinding(Label.TextProperty, ".");
        return label;
    });

    #endregion

    #region WidthProperty

    public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(GridColumnZero), new GridLength(250), BindingMode.OneWay, null, OnLayoutChanged);

    [System.ComponentModel.TypeConverter(typeof(Microsoft.Maui.Controls.GridLengthTypeConverter))]
    public GridLength Width
    {
        get { return (GridLength)GetValue(WidthProperty); }
        set { SetValue(WidthProperty, value); }
    }

    #endregion

    #region MinWidthProperty

    public static readonly BindableProperty MinWidthProperty = BindableProperty.Create(nameof(MinWidth), typeof(double), typeof(GridColumnZero), 0.0, BindingMode.OneWay, null, OnLayoutChanged);

    public double MinWidth
    {
        get { return (double)GetValue(MinWidthProperty); }
        set { SetValue(MinWidthProperty, value); }
    }

    #endregion

    #region MaxWidthProperty

    public static readonly BindableProperty MaxWidthProperty = BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(GridColumnZero), double.PositiveInfinity, BindingMode.OneWay, null, OnLayoutChanged);

    public double MaxWidth
    {
        get { return (double)GetValue(MaxWidthProperty); }
        set { SetValue(MaxWidthProperty, value); }
    }

    #endregion

    #region IsVisibleProperty

    public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(GridColumnZero), true, BindingMode.OneWay, null, OnLayoutChanged);

    public bool IsVisible
    {
        get { return (bool)GetValue(IsVisibleProperty); }
        set { SetValue(IsVisibleProperty, value); }
    }

    #endregion

    #region HeaderProperty

    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(object), typeof(GridColumnZero), null, BindingMode.OneWay, null, OnHeaderChanged);

    public object Header
    {
        get { return GetValue(HeaderProperty); }
        set { SetValue(HeaderProperty, value); }
    }

    #endregion

    #region HeaderTemplateProperty

    public static readonly BindableProperty HeaderTemplateProperty = BindableProperty.Create(nameof(HeaderTemplate), typeof(DataTemplate), typeof(GridColumnZero), null, BindingMode.OneWay, null, OnHeaderChanged);

    public DataTemplate HeaderTemplate
    {
        get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
        set { SetValue(HeaderTemplateProperty, value); }
    }

    #endregion

    #region CellStyleProperty

    public static readonly BindableProperty CellStyleProperty = BindableProperty.Create(nameof(CellStyle), typeof(Style), typeof(GridColumnZero), null, BindingMode.OneWay, null, OnTemplateChanged);

    /// <summary>
    /// Optional Style applied to every <see cref="CellZero"/> in this column.
    /// </summary>
    public Style CellStyle
    {
        get { return (Style)GetValue(CellStyleProperty); }
        set { SetValue(CellStyleProperty, value); }
    }

    #endregion

    #region BackgroundColorProperty

    public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(GridColumnZero), null, BindingMode.OneWay, null, OnTemplateChanged);

    public Color BackgroundColor
    {
        get { return (Color)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    #endregion

    #region IsTreeColumnProperty

    public static readonly BindableProperty IsTreeColumnProperty = BindableProperty.Create(nameof(IsTreeColumn), typeof(bool), typeof(GridColumnZero), false, BindingMode.OneWay, null, OnTemplateChanged);

    /// <summary>
    /// When true, cells draw an indent and expander for a TreeNodeContainer and bind the template to its Data.
    /// </summary>
    public bool IsTreeColumn
    {
        get { return (bool)GetValue(IsTreeColumnProperty); }
        set { SetValue(IsTreeColumnProperty, value); }
    }

    #endregion

    #region IndentMultiplierProperty

    public static readonly BindableProperty IndentMultiplierProperty = BindableProperty.Create(nameof(IndentMultiplier), typeof(double), typeof(GridColumnZero), 15.0, BindingMode.OneWay, null, OnTemplateChanged);

    public double IndentMultiplier
    {
        get { return (double)GetValue(IndentMultiplierProperty); }
        set { SetValue(IndentMultiplierProperty, value); }
    }

    #endregion

    private static void OnLayoutChanged(BindableObject bindable, object oldValue, object newValue)
        => ((GridColumnZero)bindable).LayoutChanged?.Invoke(bindable, EventArgs.Empty);

    private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
        => ((GridColumnZero)bindable).HeaderChanged?.Invoke(bindable, EventArgs.Empty);

    internal event EventHandler HeaderChanged;

    private static void OnTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var self = (GridColumnZero)bindable;
        self._contentCache.Clear();
        self.TemplateChanged?.Invoke(bindable, EventArgs.Empty);
    }

    internal CellZero CreateCell()
    {
        CellZero cell = IsTreeColumn ? new TreeCellZero(this) : new CellZero();
        if (CellStyle != null)
            cell.Style = CellStyle;
        if (BackgroundColor != null)
            cell.BackgroundColor = BackgroundColor;
        return cell;
    }

    /// <summary>
    /// Selects the template for an item. For tree columns the item is a TreeNodeContainer, which is what TemplateProvider expects.
    /// The selection is made once when a row is bound; it is not re-evaluated when properties on the item change.
    /// </summary>
    internal DataTemplate SelectTemplate(object item)
    {
        var template = ItemTemplate ?? DefaultTemplate;
        while (template is DataTemplateSelector selector)
            template = selector.SelectTemplate(item, null);
        return template ?? DefaultTemplate;
    }

    internal View RentContent(DataTemplate template)
    {
        if (_contentCache.TryGetValue(template, out var stack) && stack.Count > 0)
            return stack.Pop();

        return (View)template.CreateContent();
    }

    internal void ReturnContent(DataTemplate template, View content)
    {
        if (_contentCache.TryGetValue(template, out var stack) == false)
            _contentCache[template] = stack = new Stack<View>();
        stack.Push(content);
    }
}

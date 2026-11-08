namespace FunctionZero.Maui.Controls;

/// <summary>
/// Hosts the content of a single column within a <see cref="RowZero"/>.
/// Target this type with a Style to customise every cell.
/// </summary>
public class CellZero : ContentView
{
    public CellZero()
    {
        IsClippedToBounds = true;
    }

    internal DataTemplate Template { get; private set; }

    protected virtual View PresentedContent
    {
        get => Content;
        set => Content = value;
    }

    internal void SetTemplate(GridColumnZero column, DataTemplate template)
    {
        var oldTemplate = Template;
        var oldContent = PresentedContent;

        Template = template;
        PresentedContent = column.RentContent(template);

        if (oldTemplate != null && oldContent != null)
            column.ReturnContent(oldTemplate, oldContent);
    }
}

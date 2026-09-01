namespace FunctionZero.Maui.Controls;

public class MaskShape : BindableObject
{
    public static readonly BindableProperty NameProperty = BindableProperty.Create(
        nameof(Name),
        typeof(string),
        typeof(MaskShape),
        string.Empty);

    public static readonly BindableProperty PathDataProperty = BindableProperty.Create(
        nameof(PathData),
        typeof(string),
        typeof(MaskShape),
        string.Empty);

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public string PathData
    {
        get => (string)GetValue(PathDataProperty);
        set => SetValue(PathDataProperty, value);
    }
}

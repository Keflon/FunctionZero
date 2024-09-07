using FunctionZero.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShowcaseZero.Mvvm.Pages.TreeGrid;

public partial class TreeGridExperimentalPage : ContentPage
{
    public TreeGridExperimentalPage()
    {
        InitializeComponent();
    }

    private object GetTempTemplate()
    {
        var retval = new Label();
        retval.SetBinding(Label.TextProperty, ".");
        return retval;
    }
}
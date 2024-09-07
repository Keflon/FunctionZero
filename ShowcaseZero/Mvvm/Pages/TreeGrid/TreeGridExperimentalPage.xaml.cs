using FunctionZero.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShowcaseZero.Mvvm.Pages.TreeGrid;

public partial class TreeGridExperimentalPage : ContentPage
{
    public TreeGridExperimentalPage()
    {
        InitializeComponent();

        var things = new ObservableCollection<GridColumnZero>();
        //things.Add(new GridColumnZero {BackgroundColor = Colors.Red });
        //things.Add(new GridColumnZero {BackgroundColor = Colors.Green });
        //things.Add(new GridColumnZero {BackgroundColor = Colors.Blue });

        var template = new DataTemplate(GetTempTemplate);

        things.Add(new GridColumnZero() { ItemTemplate = template});
        things.Add(new GridColumnZero() { ItemTemplate = template });
        things.Add(new GridColumnZero() { ItemTemplate = template });
        gvz.ColumnsSource = things;
    }

    private object GetTempTemplate()
    {
        var retval = new Label();
        retval.SetBinding(Label.TextProperty, ".");
        return retval;
    }

    //int _busyCount = 0;

    private ListViewZero _kingOfEverything;

    //private void lvx_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{

    //    if (e.PropertyName != nameof(lv0.ScrollOffset))
    //        return;

    //    if (_kingOfEverything != null)
    //        return;

    //    this.Dispatcher.Dispatch(DoTheThing);

    //    //Debug.WriteLine(_busyCount);
    //    _kingOfEverything = (ListViewZero)sender;



    //}

    //private void DoTheThing()
    //{
    //    if (_kingOfEverything == lv0)
    //    {
    //        //lv1.ScrollView.ScrollYRequest = lv0.ScrollOffset;

    //        lv1.ScrollView.ScrollYRequest = lv0.ScrollOffset;
    //        lv2.ScrollView.ScrollYRequest = lv0.ScrollOffset;
    //    }
    //    else if (_kingOfEverything == lv1)
    //    {
    //        lv0.ScrollView.ScrollYRequest = lv1.ScrollOffset;
    //        lv2.ScrollView.ScrollYRequest = lv1.ScrollOffset;
    //    }
    //    else if (_kingOfEverything == lv2)
    //    {
    //        lv0.ScrollView.ScrollYRequest = lv2.ScrollOffset;
    //        lv1.ScrollView.ScrollYRequest = lv2.ScrollOffset;
    //    }
    //    else
    //    {
    //        throw new InvalidOperationException("What?");
    //    }
    //    _kingOfEverything = null;
    //    //_busyCount--;
    //}
}
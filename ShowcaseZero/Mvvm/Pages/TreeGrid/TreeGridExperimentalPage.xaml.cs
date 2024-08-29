using FunctionZero.Maui.Controls;
using System.Diagnostics;

namespace ShowcaseZero.Mvvm.Pages.TreeGrid;

public partial class TreeGridExperimentalPage : ContentPage
{
    public TreeGridExperimentalPage()
    {
        InitializeComponent();
    }

    //int _busyCount = 0;

    private ListViewZero _kingOfEverything;

    private void lvx_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {

        if (e.PropertyName != nameof(lv0.ScrollOffset))
            return;

        if (_kingOfEverything != null)
            return;

        this.Dispatcher.Dispatch(DoTheThing);

        //Debug.WriteLine(_busyCount);
        _kingOfEverything = (ListViewZero)sender;



    }

    private void DoTheThing()
    {
        if (_kingOfEverything == lv0)
        {
            lv1.ScrollOffset = lv0.ScrollOffset;

            lv1.ScrollOffset = lv0.ScrollOffset;
            lv2.ScrollOffset = lv0.ScrollOffset;
        }
        else if (_kingOfEverything == lv1)
        {
            lv0.ScrollOffset = lv1.ScrollOffset;
            lv2.ScrollOffset = lv1.ScrollOffset;
        }
        else if (_kingOfEverything == lv2)
        {
            lv0.ScrollOffset = lv2.ScrollOffset;
            lv1.ScrollOffset = lv2.ScrollOffset;
        }
        else
        {
            throw new InvalidOperationException("What?");
        }
        _kingOfEverything = null;
        //_busyCount--;
    }
}
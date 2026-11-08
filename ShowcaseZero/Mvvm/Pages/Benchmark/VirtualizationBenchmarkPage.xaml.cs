using FunctionZero.Maui.Controls;
using ShowcaseZero.Mvvm.PageViewModels.Benchmark;
using System.Diagnostics;

namespace ShowcaseZero.Mvvm.Pages.Benchmark;

public partial class VirtualizationBenchmarkPage : ContentPage
{
    private const int FrameCount = 600;
    private const double PixelsPerFrame = 120.0;

    public VirtualizationBenchmarkPage()
    {
        InitializeComponent();
        ItemCountPicker.SelectedIndex = 0;
        ColumnCountPicker.SelectedIndex = 1;
    }

    private VirtualizationBenchmarkPageVm Vm => (VirtualizationBenchmarkPageVm)BindingContext;

    private async void Run_Clicked(object sender, EventArgs e)
    {
        int itemCount = int.Parse((string)ItemCountPicker.SelectedItem);
        int columnCount = int.Parse((string)ColumnCountPicker.SelectedItem);

        Vm.Results = "Running...";
        Vm.CreateItems(itemCount);

        var gridView = new GridViewZero { ItemHeight = 30, HeaderVisible = true, ColumnSpacing = 4, SelectionMode = SelectionMode.Single };
        for (int c = 0; c < columnCount; c++)
            gridView.ColumnsSource.Add(new GridColumnZero { Header = $"Column {c}", ItemTemplate = CreateCellTemplate(c) });

        gridView.ItemsSource = Vm.Items;
        Host.Content = gridView;

        // Allow the first layout and realization to complete.
        await Task.Delay(500);

        var frameTimes = new List<double>(FrameCount);
        var updateTimes = new List<double>(FrameCount);
        var frameClock = Stopwatch.StartNew();
        var updateClock = new Stopwatch();
        double maxOffset = itemCount * gridView.ItemHeight - gridView.Height;
        double offset = 0;
        double direction = 1;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            offset += PixelsPerFrame * direction;
            if (offset > maxOffset || offset < 0)
            {
                direction = -direction;
                offset = Math.Clamp(offset, 0, maxOffset);
            }

            updateClock.Restart();
            gridView.ScrollOffset = offset;
            updateClock.Stop();
            updateTimes.Add(updateClock.Elapsed.TotalMilliseconds);

            // Yield so the dispatcher can run deferred work and render a frame.
            await Task.Delay(1);
            frameTimes.Add(frameClock.Elapsed.TotalMilliseconds);
            frameClock.Restart();
        }

        Vm.Results = $"{itemCount} items, {columnCount} columns, {FrameCount} frames | " +
                     $"Frame avg {frameTimes.Average():F2} ms, p95 {Percentile(frameTimes, 0.95):F2} ms, max {frameTimes.Max():F2} ms | " +
                     $"Update avg {updateTimes.Average():F3} ms, max {updateTimes.Max():F3} ms";
        Debug.WriteLine($"[Benchmark] {Vm.Results}");
    }

    private static DataTemplate CreateCellTemplate(int columnIndex)
    {
        return new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, columnIndex % 2 == 0 ? nameof(BenchmarkItem.Name) : nameof(BenchmarkItem.Value));
            return label;
        });
    }

    private static double Percentile(List<double> values, double percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int index = Math.Clamp((int)Math.Ceiling(percentile * sorted.Count) - 1, 0, sorted.Count - 1);
        return sorted[index];
    }
}

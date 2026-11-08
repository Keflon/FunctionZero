namespace FunctionZero.Maui.Controls
{
    /// <summary>
    /// Resizes a <see cref="GridColumnZero"/> by dragging. Lives in the GridViewZero header.
    /// </summary>
    public class GridSplitterZero : ContentView
    {
        private readonly GridColumnZero _column;
        private double _startWidth;

        internal GridSplitterZero(GridColumnZero column)
        {
            _column = column;
            // Nearly transparent rather than Transparent so the platform hit-tests the whole grab area.
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.01);
            Content = new BoxView { Color = Colors.Gray, WidthRequest = 1, HorizontalOptions = LayoutOptions.Center };
        }

        internal event EventHandler Resizing;

        internal double CurrentWidth { get; set; }

        internal GridColumnZero Column => _column;

        internal void BeginResize()
        {
            _startWidth = CurrentWidth;
            Opacity = 0.5;
        }

        internal void Resize(double totalX)
        {
            _column.Width = new GridLength(Math.Max(0, _startWidth + totalX));
            Resizing?.Invoke(this, EventArgs.Empty);
        }

        internal void EndResize() => Opacity = 1.0;
    }
}

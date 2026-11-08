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
            BackgroundColor = Colors.Transparent;
            Content = new BoxView { Color = Colors.Gray, WidthRequest = 1, HorizontalOptions = LayoutOptions.Center };

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += Pan_PanUpdated;
            GestureRecognizers.Add(pan);
        }

        internal event EventHandler Resizing;

        internal double CurrentWidth { get; set; }

        private void Pan_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startWidth = CurrentWidth;
                    Opacity = 0.5;
                    break;
                case GestureStatus.Running:
                    _column.Width = new GridLength(Math.Max(0, _startWidth + e.TotalX));
                    Resizing?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    Opacity = 1.0;
                    break;
            }
        }
    }
}

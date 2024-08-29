namespace FunctionZero.Maui.Controls
{
    public class GridSplitterZero : ContentView
    {
        private int _columnIndexToResize;
        private DropGestureRecognizer _dropGestureRecognizer;

        public GridSplitterZero()
        {
            var dragGestureRecognizer = new DragGestureRecognizer() { CanDrag = true };
            dragGestureRecognizer.DragStarting += DragGestureRecognizer_DragStarting;
            dragGestureRecognizer.DropCompleted += DragGestureRecognizer_DropCompleted;
            GestureRecognizers.Add(dragGestureRecognizer);

            _dropGestureRecognizer = new DropGestureRecognizer();
            _dropGestureRecognizer.DragOver += _dropGestureRecognizer_DragOver;
        }

        private void _dropGestureRecognizer_DragOver(object? sender, DragEventArgs e)
        {
#if WINDOWS
            var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
            dragUI.IsCaptionVisible = false;
            dragUI.IsGlyphVisible = false;
            dragUI.IsContentVisible = false;
#endif
            var parentGrid = Parent as Grid;
            double newWidth = (int)e.GetPosition(parentGrid)!.Value.X;

            for (int c = 0; c < _columnIndexToResize; c++)
                newWidth -= parentGrid.ColumnDefinitions[c].Width.Value;

            if (newWidth < 0)
                newWidth = 0;

            parentGrid.ColumnDefinitions[_columnIndexToResize].Width = new GridLength(newWidth);
            ForceGridRedraw(parentGrid);
        }

        private void ForceGridRedraw(Grid parentGrid)
        {
            this.InvalidateLayout();
        }

        private void DragGestureRecognizer_DragStarting(object? sender, DragStartingEventArgs e)
        {
            Opacity = 0.5;
            var parentGrid = Parent as Grid;
            parentGrid.GestureRecognizers.Add(_dropGestureRecognizer);
            _columnIndexToResize = Grid.GetColumn(this) - 1;
        }

        private void DragGestureRecognizer_DropCompleted(object? sender, DropCompletedEventArgs e)
        {
            Opacity = 1.0;
            var parentGrid = Parent as Grid;
            parentGrid.GestureRecognizers.Remove(_dropGestureRecognizer);
            ForceGridRedraw(parentGrid);
        }
    }
}

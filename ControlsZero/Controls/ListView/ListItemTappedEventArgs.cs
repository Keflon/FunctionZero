namespace FunctionZero.Maui.Controls
{
    public class ListItemTappedEventArgs : EventArgs
    {
        public ListItemTappedEventArgs(ListItemZero tappedItem)
        {
            TappedItem = tappedItem;
        }

        public ListItemZero TappedItem { get; }
    }
}
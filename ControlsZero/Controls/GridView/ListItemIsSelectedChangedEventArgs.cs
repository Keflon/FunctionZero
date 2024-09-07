namespace FunctionZero.Maui.Controls
{
    public class ListItemIsSelectedChangedEventArgs : EventArgs
    {
        public ListItemIsSelectedChangedEventArgs(ListItemZero instance)
        {
            Instance = instance;
        }

        public ListItemZero Instance { get; }
    }
}
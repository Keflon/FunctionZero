namespace ShowcaseZero.Services.BasicTreeNode
{
    public class ChildCountChangedEventArgs : EventArgs
    {
        public ChildCountChangedEventArgs(int level, int newCount)
        {
            Level = level;
            NewCount = newCount;
        }

        public int Level { get; }
        public int NewCount { get; }
    }
}
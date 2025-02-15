namespace ShowcaseZero.Services.BasicTreeNode
{
    public class TunnelEventArgs : EventArgs
    {
        public TunnelEventArgs(Func<BasicTreeNode, bool> predicate, Action<BasicTreeNode> action)
        {
            Predicate = predicate;
            Action = action;
        }

        public Func<BasicTreeNode, bool> Predicate { get; }
        public Action<BasicTreeNode> Action { get; }
    }
}
using System;

namespace FunctionZero.TreeZero
{
    public class ParentChangedEventArgs<T> : EventArgs where T : INode<T>
    {
        public ParentChangedEventArgs(T oldParent, T newParent)
        {
            OldParent = oldParent;
            NewParent = newParent;
        }

        public T OldParent { get; }
        public T NewParent { get; }
    }
}
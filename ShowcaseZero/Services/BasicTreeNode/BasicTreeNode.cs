using FunctionZero.TreeZero;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Services.BasicTreeNode
{
    public class BasicTreeNode : Node<BasicTreeNode>
    {
        public string Name { get; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded; 
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public BasicTreeNode(string name)
        {
            this.Children.CollectionChanged += Children_CollectionChanged;
            Name = name;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var child in e.NewItems!)
                        NodeAdded((BasicTreeNode)child);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var child in e.OldItems!)
                        NodeRemoved(child);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new InvalidOperationException();
            }
        }

        protected virtual void NodeAdded(BasicTreeNode child)
        {
            Parent?.NodeAdded(child);
        }

        protected virtual void NodeRemoved(object child)
        {
            Parent?.NodeRemoved(child);
        }

        protected virtual void AcceptTunnelEvent(TunnelEventArgs e)
        {
            if (e.Predicate(this))
                e.Action(this);

            foreach (var child in this.Children)
                child.AcceptTunnelEvent(e);
        }
    }
}

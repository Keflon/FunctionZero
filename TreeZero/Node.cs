using FunctionZero.TreeZero.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FunctionZero.TreeZero
{
    public abstract class Node<T> : INode<T> where T : Node<T>
    {
        private T _parent;

        //public Node(ObservableCollection<T> children = null)
        //{
        //    if (children != null)
        //    {
        //        Children = children;
        //        foreach (var child in children)
        //        {
        //            if (child.Parent != null)
        //                throw new TreeZeroException(this, ExceptionReason.CollectionItemAlreadyParented);
        //            child._changeInProgress = true;
        //            child.Parent = (T)this;
        //            child._changeInProgress = false;
        //        }
        //    }
        //    else
        //        Children = new ObservableCollection<T>();

        //    Children.CollectionChanged += Children_CollectionChanged;
        //}

        public Node(ObservableCollection<T> children = null)
        {
            Children = children ?? new ObservableCollection<T>();

            foreach (var child in Children)
            {
                if (child.Parent != null)
                    throw new TreeZeroException(this, ExceptionReason.CollectionItemAlreadyParented);
                child._changeInProgress = true;
                child.Parent = (T)this;
                child._changeInProgress = false;
            }

            Children.CollectionChanged += Children_CollectionChanged;
        }

        public ObservableCollection<T> Children { get; }

        public T Parent
        {
            get => _parent;
            set
            {
                if (_parent != value)
                {
                    if (value == this)
                        throw new TreeZeroException(this, ExceptionReason.ParentToSelf);
                    if (value != null && value.IsChildOf(this))
                        throw new TreeZeroException(this, ExceptionReason.ParentToDescendant);
                    _parent = value;
                    OnPropertyChanged();
                }
            }
        }


        //public int NestLevel
        //{
        //    get => Parent?.NestLevel + 1 ?? 0;
        //}

        public int NestLevel
        {
            get
            {
                int level = 0;

                Node<T> current = this;
                while (current != null)
                {
                    current = current.Parent;
                    level++;
                }
                return level;
            }
        }

        private bool _changeInProgress = false;
        private T _oldParent;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(Parent))
            {
                if (_changeInProgress == false)
                {
                    _changeInProgress = true;
                    if (_oldParent != null)
                        _oldParent.Children.Remove((T)this);

                    if (Parent != null)
                        Parent.Children.Add((T)this);

                    _changeInProgress = false;
                }
                _oldParent = Parent;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count != 1)
                        throw new InvalidOperationException("Cannot add multiple items to Node children at once");
                    var newItem = (T)e.NewItems[0];

                    if (newItem._changeInProgress == false)
                    {
                        if (newItem.Parent != null)
                        {
                            if (newItem.Parent == this)
                                throw new TreeZeroException(this, ExceptionReason.ChildAddedToSameParent);

                            newItem._changeInProgress = true;
                            newItem.Parent.Children.Remove(newItem);
                            newItem._changeInProgress = false;
                        }
                        //Debug.WriteLine(newItem);

                        newItem._changeInProgress = true;
                        newItem.Parent = (T)this;
                        newItem._changeInProgress = false;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count != 1)
                        throw new InvalidOperationException("Cannot remove multiple items from Node children at once");
                    var oldItem = (T)e.OldItems[0];

                    if (oldItem._changeInProgress == false)
                    {
                        oldItem._changeInProgress = true;
                        oldItem.Parent = null;
                        oldItem._changeInProgress = false;
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }


        public bool IsChildOf(INode<T> item)
        {
            if (this.Parent == null)
                return false;
            if (this.Parent == item)
                return true;
            return this.Parent.IsChildOf(item);
        }
        public bool IsAncestorOf(INode<T> item)
        {
            if (item.Parent == null)
                return false;
            if (item.Parent == this)
                return true;
            return item.Parent.IsChildOf(this);
        }
        //public static IEnumerable<Node<T>> AllChildren(Node<T> root)
        //{
        //    Queue<Node<T>> nodeQueue = new Queue<Node<T>>();

        //    nodeQueue.Enqueue(root);

        //    while (nodeQueue.Count != 0)
        //    {
        //        var nextItem = nodeQueue.Dequeue();

        //        yield return nextItem;

        //        foreach (var nextChildItem in nextItem.Children)
        //        {
        //            nodeQueue.Enqueue(nextChildItem);
        //        }
        //    }
        //}

        public static IEnumerable<INode<T>> AllChildren(INode<T> root, bool includeRoot = true)
        {
            if (includeRoot)
                yield return root;

            foreach (var nextChildItem in root.Children)
            {
                foreach (var item in AllChildren(nextChildItem))
                {
                    yield return item;
                }
            }
        }
    }
}


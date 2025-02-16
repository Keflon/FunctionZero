using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Services.BasicTreeNode
{
    public class BasicRootNode : BasicTreeNode
    {
        public event EventHandler<ChildCountChangedEventArgs>? ChildCountChanged;

        private int _level0ChildCount;
        private int _level1ChildCount;
        private int _level2ChildCount;
        private int _level3ChildCount;

        public int Level0ChildCount
        {
            get => _level0ChildCount;
            set
            {
                if (_level0ChildCount != value)
                {
                    _level0ChildCount = value;
                    ChildCountChanged?.Invoke(this, new ChildCountChangedEventArgs(0, Level0ChildCount));
                    OnPropertyChanged();
                }
            }
        }
        public int Level1ChildCount
        {
            get => _level1ChildCount;
            set
            {
                if (_level1ChildCount != value)
                {
                    _level1ChildCount = value;
                    ChildCountChanged?.Invoke(this, new ChildCountChangedEventArgs(1, Level1ChildCount));
                }
            }
        }
        public int Level2ChildCount
        {
            get => _level2ChildCount;
            set
            {
                if (_level2ChildCount != value)
                {
                    _level2ChildCount = value;
                    ChildCountChanged?.Invoke(this, new ChildCountChangedEventArgs(2, Level2ChildCount));
                }
            }
        }
        public int Level3ChildCount
        {
            get => _level3ChildCount;
            set
            {
                if (_level3ChildCount != value)
                {
                    _level3ChildCount = value;
                    ChildCountChanged?.Invoke(this, new ChildCountChangedEventArgs(3, Level3ChildCount));
                }
            }
        }

        public BasicRootNode(string name) : base(name)
        {
            ChildCountChanged += BasicRootNode_ChildCountChanged;
        }

        private void BasicRootNode_ChildCountChanged(object? sender, ChildCountChangedEventArgs e)
        {
            AcceptTunnelEvent(new TunnelEventArgs((child) => child.NestLevel-1 == e.Level, ManageChildren));
        }

        protected override void NodeAdded(BasicTreeNode child)
        {
            ManageChildren(child);
        }

        private void ManageChildren(BasicTreeNode child)
        {
            int numChildren = GetNumChildren(child.NestLevel-1);

            while (child.Children.Count < numChildren)
            {
                var newChild = new BasicTreeNode($"{child.NestLevel}-{child.Children.Count}");
                child.Children.Add(newChild);
                newChild.IsExpanded = true;
            }
            while (child.Children.Count > numChildren)
                child.Children.RemoveAt(0);
        }

        protected override void NodeRemoved(object child)
        {
        }

        private int GetNumChildren(int nestLevel)
        {
            return nestLevel switch
            {
                0 => Level0ChildCount,
                1 => Level1ChildCount,
                2 => Level2ChildCount,
                3 => Level3ChildCount,
                _ => 0,
            };
        }
    }
}

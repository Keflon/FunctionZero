using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FunctionZero.TreeZero
{
    public interface INode<T> : INotifyPropertyChanged where T : INode<T>
    {
        ObservableCollection<T> Children { get; }
        int NestLevel { get; }
        T Parent { get; set; }

        bool IsAncestorOf(INode<T> item);
        bool IsChildOf(INode<T> item);
    }
}
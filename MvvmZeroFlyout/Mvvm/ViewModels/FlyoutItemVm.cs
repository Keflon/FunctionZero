using FunctionZero.TreeZero;
using System.Windows.Input;

namespace MvvmZeroFlyout.Mvvm.ViewModels
{
    public class FlyoutItemVm : Node<FlyoutItemVm>
    {
        public FlyoutItemVm(string title, Action<object> action)
        {
            Title = title;
            action = action ?? DefaultAction;
            TappedCommand = new Command(action);
        }

        public string Title { get; }

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

        public ICommand TappedCommand { get; }

        private void DefaultAction(object obj)
        {
            if (Children.Count != 0)
                IsExpanded = !IsExpanded;
        }
    }
}
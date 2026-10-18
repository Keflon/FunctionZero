using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ShowcaseZero.Wpf;

public sealed class MainViewVm : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer;
    private int _counter;

    public MainViewVm()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromSeconds(1.0 / 60.0)
        };
        _timer.Tick += (_, _) => Counter++;
        _timer.Start();
    }

    public int Counter
    {
        get => _counter;
        private set
        {
            if (_counter == value)
                return;

            _counter = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

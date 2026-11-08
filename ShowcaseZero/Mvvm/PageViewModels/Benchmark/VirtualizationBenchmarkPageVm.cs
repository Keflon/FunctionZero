using System.Collections;
using System.Collections.ObjectModel;

namespace ShowcaseZero.Mvvm.PageViewModels.Benchmark
{
    public class BenchmarkItem
    {
        public BenchmarkItem(int index)
        {
            Index = index;
            Name = $"Item {index}";
            Value = Math.Sin(index / 20.0) * 100.0;
        }

        public int Index { get; }
        public string Name { get; }
        public double Value { get; }
    }

    public class VirtualizationBenchmarkPageVm : BasePageVm
    {
        private IList _items;
        private string _results = "Choose an item count and column count, then press Run.";

        public IList Items
        {
            get => _items;
            set { _items = value; OnPropertyChanged(); }
        }

        public string Results
        {
            get => _results;
            set { _results = value; OnPropertyChanged(); }
        }

        public void CreateItems(int count)
        {
            var list = new List<BenchmarkItem>(count);
            for (int c = 0; c < count; c++)
                list.Add(new BenchmarkItem(c));

            Items = new ObservableCollection<BenchmarkItem>(list);
        }
    }
}

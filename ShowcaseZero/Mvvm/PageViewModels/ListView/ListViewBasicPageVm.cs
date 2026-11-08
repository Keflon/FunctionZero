using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.PageViewModels.ListView
{

    public class BasicListItem
    {
        public BasicListItem(string name, double value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public double Value { get; }
    }

    public class ListViewBasicPageVm : BasePageVm
    {
        public ObservableCollection<BasicListItem> Items { get; }

        public ListViewBasicPageVm()
        {
            Items = new ObservableCollection<BasicListItem>();

            for (int c = 0; c < 1000; c++)
            {
                Items.Add(new BasicListItem($"Item {c}", Math.Sin(c / 20.0) * 20.0 + 20.0));
            }
        }
    }
}

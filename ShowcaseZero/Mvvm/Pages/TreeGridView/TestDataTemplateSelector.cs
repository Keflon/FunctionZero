using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.Pages.TreeGrid
{
    public class TestDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate First { get; set; }
        public DataTemplate Second { get; set; }
        int _count = 0;
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            _count++;
            if((_count & 1)==0)
                return First;
            return Second;
        }
    }
}

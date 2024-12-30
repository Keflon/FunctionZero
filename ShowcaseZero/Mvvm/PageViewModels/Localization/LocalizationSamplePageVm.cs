using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.PageViewModels.Localization
{
    public class LocalizationSamplePageVm : BasePageVm
    {
        private int _numCats;
        public int NumCats { get => _numCats; set => SetProperty(ref _numCats, value); }
        public LocalizationSamplePageVm()
        {
            AddPageTimer(1000, Tick, null, null);
        }

        private void Tick(object obj)
        {
            NumCats++;
            if (NumCats == 10)
                NumCats = 0;
        }
    }
}

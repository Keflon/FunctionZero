using FunctionZero.Maui.MvvmZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmZeroFlyout.Mvvm.PageViewModels
{
    public class DetailThreePageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;

        public DetailThreePageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;
        }
    }
}

using FunctionZero.Maui.MvvmZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmZeroFlyout.Mvvm.PageViewModels
{
    public class DetailOnePageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;

        public DetailOnePageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;
        }
    }
}

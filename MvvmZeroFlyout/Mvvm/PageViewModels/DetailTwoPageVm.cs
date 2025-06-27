using FunctionZero.Maui.MvvmZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmZeroFlyout.Mvvm.PageViewModels
{
    internal class DetailTwoPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;

        public DetailTwoPageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;
        }
    }
}

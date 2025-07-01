using FunctionZero.CommandZero;
using FunctionZero.Maui.MvvmZero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmZeroTabbedPage.Mvvm.PageViewModels
{
    public class ReadyPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;
        public string InitMessage { get; private set; }

        public ReadyPageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;
            InitMessage = "Uninitialised";


        }


        internal void Init(string initMessage)
        {
            InitMessage = initMessage;
        }
    }
}
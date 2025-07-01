using FunctionZero.CommandZero;
using FunctionZero.Maui.MvvmZero;
using System.Windows.Input;

namespace MvvmZeroTabbedPage.Mvvm.PageViewModels
{
    public class DetailPageVm
    {
        private readonly IPageServiceZero _pageService;
        public ICommand PushDetailPageCommand { get; }
        public string Message { get; private set; }
        public int NestLevel { get; private set; }
        public DetailPageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;

            Message = "Uninitialised";

            PushDetailPageCommand = new CommandBuilder()
            .SetName("PushDetailPageCommand")
            .SetCanExecute(PushDetailPageCommandCanExecute)
            .SetExecuteAsync(PushDetailPageCommandExecuteAsync)
            .Build();
        }

        private bool PushDetailPageCommandCanExecute(object arg)
        {
            return true;
        }
        private async Task PushDetailPageCommandExecuteAsync(object arg)
        {
            await _pageService.PushVmAsync<DetailPageVm>(vm => vm.Init("Pushed from DetailPageVm", NestLevel + 1));
        }
        internal void Init(string message, int nestLevel)
        {
            Message = message;
            NestLevel = nestLevel;
        }
    }
}

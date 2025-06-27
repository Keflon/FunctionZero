using FunctionZero.CommandZero;
using FunctionZero.Maui.MvvmZero;
using System.Windows.Input;


namespace MvvmZeroFlyout.Mvvm.PageViewModels
{
    public class GoPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;
        public ICommand PushDetailPageCommand { get; }
        public string InitMessage { get; private set; }

        public GoPageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;
            InitMessage = "Uninitialised";

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
            await _pageService.PushVmAsync<DetailPageVm>(vm => vm.Init("Pushed from GoPageVm", 1));
        }

        internal void Init(string initMessage)
        {
            InitMessage = initMessage;
        }
    }
}

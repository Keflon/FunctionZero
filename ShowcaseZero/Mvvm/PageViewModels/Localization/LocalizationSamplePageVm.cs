using FunctionZero.CommandZero;
using Microsoft.VisualBasic;
using ShowcaseZero.Localization;
using ShowcaseZero.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShowcaseZero.Mvvm.PageViewModels.Localization
{
    public class LocalizationSamplePageVm : BasePageVm
    {
        private int _numCats;
        public int NumCats { get => _numCats; set => SetProperty(ref _numCats, value); }
        private int _numDogs;
        public int NumDogs { get => _numDogs; set => SetProperty(ref _numDogs, value); }
        public ObservableCollection<LocalizationItemVm> LocalizationItems { get; }

        public ICommand SetLanguageCommand { get; }

        private string _currentFlagAssetName;
        public string CurrentFlagAssetName { get => _currentFlagAssetName; set => SetProperty(ref _currentFlagAssetName, value); }
        private string _currentLanguageCode;
        public string CurrentLanguageCode { get => _currentLanguageCode; set => SetProperty(ref _currentLanguageCode, value); }


        public LocalizationSamplePageVm(LocalizationService localization)
        {
            SetLanguageCommand = new CommandBuilder()
                .AddGuard(this)
                .SetExecuteAsync((arg) => localization.SetLanguageAsync((string)arg))
                .Build();

            localization.LanguageChanged += (s, e) => SetLanguageDependentStrings(localization);
            SetLanguageDependentStrings(localization);

            //AddPageTimer(1000, Tick, null, null);

            // TODO: Make the LanguageService responsible for providing this info.
            LocalizationItems = new ObservableCollection<LocalizationItemVm>
            {
                new LocalizationItemVm("de.png",  "Deutsch"  , "german"            ),
                new LocalizationItemVm("en.png",  "English"  , "english"           ),
                new LocalizationItemVm("es.png",  "Española" , "spanish"           ),
                new LocalizationItemVm("fr.png",  "Française", "french"            ),
                new LocalizationItemVm("it.png",  "Italiana" , "italian"           ),
                new LocalizationItemVm("ja.png",  "日本語"    , "japanese"          ),
                new LocalizationItemVm("nl.png", "Nederlands", "dutch"             ),
                new LocalizationItemVm("pt.png",  "Portugese", "portugese"         ),
                new LocalizationItemVm("ru.png",  "русский"  , "russian"           ),
                new LocalizationItemVm("sv.png",  "Svenska"  , "swedish"           ),
                new LocalizationItemVm("zh.png", "中文"       , "chinese-simplified"),
            };
        }

        private void SetLanguageDependentStrings(LocalizationService langService)
        {
            CurrentFlagAssetName = langService.GetText(LocalizationStrings._E_FlagAssetName, null);
            CurrentLanguageCode = langService.GetText(LocalizationStrings._E_CountryCode, null);
        }



        private void Tick(object obj)
        {
            NumCats++;
            if (NumCats == 10)
                NumCats = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.ViewModels
{
    public class LocalizationItemVm : BaseVm
    {
        public LocalizationItemVm(string imageName, string text, string languageName)
        {
            ImageName = imageName;
            Text = text;
            LanguageName = languageName;
        }

        public string ImageName { get; }
        public string Text { get; }
        public string LanguageName { get; }
    }
}

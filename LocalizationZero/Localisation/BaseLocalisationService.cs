using System.ComponentModel;

namespace LocalisationZero.Localisation
{
    /// <summary>
    /// Goal. To be decoupled enough to allow downloading and selection of new or updated language packs on the fly.
    /// </summary>
    public abstract partial class BaseLocalisationService<TEnum> : INotifyPropertyChanged where TEnum : Enum
    {
        private readonly string _resourceKey;
        private ResourceDictionary _resourceHost;
        private Dictionary<string, LocalisationProvider> _languages;
        public event EventHandler<LocalisationChangedEventArgs> LanguageChanged;

        // INPC raised by SetLanguage(..)
        public string CurrentLanguageId { get; protected set; }


        public event PropertyChangedEventHandler PropertyChanged;


        public BaseLocalisationService(string resourceKey = "languageResource")
        {
            _resourceKey = resourceKey;
            _languages = new();
        }

        public void Init(ResourceDictionary resourceHost, string initialLanguage)
        {
            _resourceHost = resourceHost;
            SetLanguage(initialLanguage);
        }

        public void RegisterLanguage(string id, LocalisationProvider language)
        {
            _languages[id] = language;
        }

        /// <summary>
        /// You probably want resourceHost to be 'Application.Current.Resources'
        /// </summary>
        /// <param name="resourceHost"></param>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        public void SetLanguage(string id)
        {
            if (_resourceHost == null)
                throw new InvalidOperationException("You must call Init on the LanguageService before you call SetLanguage(string id), e.g. Init(Application.Current.Resources);");

            if (_languages.TryGetValue(id, out var languageService))
                _resourceHost[_resourceKey] = languageService.GetLookup();
            else
                throw new Exception($"Register a language before trying to set it. Language: '{id}' ");

            CurrentLanguageId = id;

            LanguageChanged?.Invoke(this, new LocalisationChangedEventArgs(id));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageId)));
        }

        //public string[] CurrentLookup => _resourceHost[_resourceKey] as string[];


        public string GetText(TEnum textId, params object[]arguments)
        {
            if (_resourceHost == null)
                throw new InvalidOperationException("You must call Init on the LanguageService before you call SetLanguage(string id), e.g. Init(Application.Current.Resources);");

            LocalisationProvider currentLanguage = _languages[CurrentLanguageId];
            return currentLanguage.GetLookup().GetString((int)(object)textId, arguments);
        }
    }
}

namespace LocalizationZero.Localization
{
    public class LocalizationChangedEventArgs : EventArgs
    {
        public LocalizationChangedEventArgs(string languageId)
        {
            LanguageId = languageId;
        }

        public string LanguageId { get; }
    }
}
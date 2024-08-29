namespace LocalisationZero.Localisation
{
    public class LocalisationChangedEventArgs : EventArgs
    {
        public LocalisationChangedEventArgs(string languageId)
        {
            LanguageId = languageId;
        }

        public string LanguageId { get; }
    }
}
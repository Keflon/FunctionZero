namespace LocalizationZero.Factory
{
    internal class LocalizationPackProxy
    {
        public LocalizationPackProxy(string language, string friendlyName)
        {
            Language = language;
            FriendlyName = friendlyName;
        }

        public string Language { get; }
        public string FriendlyName { get; }
    }
}
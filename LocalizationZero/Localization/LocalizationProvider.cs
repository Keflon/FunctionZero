using LocalizationZero.Factory;
using System.Collections.ObjectModel;
using System.Xml;
namespace LocalizationZero.Localization
{
    public class LocalizationProvider
    {
        public LocalizationProvider(Func<Stream> inputStreamGetter)
        {
            using var stream = inputStreamGetter();
            using var reader = new XmlTextReader(stream);
            {
                var factory = new LocalizationPackFactory();

                factory.ResetState();

                var result = ((LocalizationPack LocalizationPack, string languageName))FunctionZero.ObjectGraphZero.XmlDeserializer.BuildObjectGraphFromXml(reader, factory).Result;

                GetLookup = () => result.LocalizationPack;
                LanguageName = result.languageName;
            }
        }
        public LocalizationProvider(Func<LocalizationPack> getLookup, string languageName)
        {
            GetLookup = getLookup;
            LanguageName = languageName;
        }
        public LocalizationProvider(Func<IEnumerable<string>> getLookup, string languageName)
        {
            GetLookup = () => GetLocalizationPackFromStringList(getLookup);
            LanguageName = languageName;
        }


        private LocalizationPack GetLocalizationPackFromStringList(Func<IEnumerable<string>> getLookup)
        {
            // LocalizationPack
            //    LocalizationRecord      each translation
            //        LocalizationItem    each translation option

            List<LocalizationRecord> recordList = new List<LocalizationRecord>();// { new LocalizationRecord(localizationItemList) };

            foreach (var localString in getLookup())
            {
                var localizationItemList = new List<LocalizationItem>();

                localizationItemList.Add(new LocalizationItem("True", localString));
                recordList.Add(new LocalizationRecord(localizationItemList));
            }

            return new LocalizationPack(recordList);
        }




        public Func<LocalizationPack> GetLookup { get; }
        public string LanguageName { get; }
    }
}

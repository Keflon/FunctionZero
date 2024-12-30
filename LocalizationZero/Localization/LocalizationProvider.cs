using LocalizationZero.Factory;
using System.Collections.ObjectModel;
using System.Xml;
namespace LocalizationZero.Localization
{
    public class LocalizationProvider
    {
        public LocalizationProvider(Func<Task<Stream>> xmlInputStreamGetter, string localizedName)
        {
            GetLocalizationPackAsync = async () =>
            {
                using var stream = await xmlInputStreamGetter();
                using var reader = new XmlTextReader(stream);
                {
                    var factory = new LocalizationPackFactory();
                    return (LocalizationPack)FunctionZero.ObjectGraphZero.XmlDeserializer.BuildObjectGraphFromXml(reader, factory).Result;
                }
            };
            LocalizedName = localizedName;
        }

        public LocalizationProvider(Func<LocalizationPack> getLookup, string localizedName)
        {
            GetLocalizationPackAsync = async () => getLookup();
            LocalizedName = localizedName;
        }

        public LocalizationProvider(Func<IEnumerable<string>> getLookup, string localizedName)
        {
            GetLocalizationPackAsync = async () => GetLocalizationPackFromStringList(getLookup, localizedName);
            LocalizedName = localizedName;
        }


        private LocalizationPack GetLocalizationPackFromStringList(Func<IEnumerable<string>> getLookup, string localizedName)
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

            return new LocalizationPack(recordList, LocalizedName);
        }




        public Func<Task<LocalizationPack>> GetLocalizationPackAsync { get; }
        public string LocalizedName { get; }
    }
}

using FunctionZero.ExpressionParserZero.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationZero.Localization
{
    public class LocalizationPack
    {
        public LocalizationPack(IList<LocalizationRecord> localizationRecords, string languageName)
        {
            LocalizationRecords = localizationRecords;
            LanguageName = languageName;
        }

        public IList<LocalizationRecord> LocalizationRecords { get; }
        public string LanguageName { get; }

        public int RecordCount => LocalizationRecords.Count;

        public string GetString(int index, object[] arguments)
        {
            var record = LocalizationRecords[index];

            var retval = record.GetText(arguments);

            return retval;
        }
    }
}

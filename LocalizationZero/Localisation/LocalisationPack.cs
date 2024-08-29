using FunctionZero.ExpressionParserZero.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalisationZero.Localisation
{
    public class LocalisationPack
    {
        public LocalisationPack(IList<LocalisationRecord> localisationRecords)
        {
            LocalisationRecords = localisationRecords;
        }

        public IList<LocalisationRecord> LocalisationRecords { get; }
        public int RecordCount => LocalisationRecords.Count;

        public string GetString(int index, object[] arguments)
        {
            var record = LocalisationRecords[index];

            var retval = record.GetText(arguments);

            return retval;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationZero.Localization
{
    public class LocalizationItem
    {
        public LocalizationItem(string conditionExpression, string localisedText)
        {
            ConditionExpression = conditionExpression;
            LocalisedText = localisedText;
        }

        public string ConditionExpression { get; }
        public string LocalisedText { get; }
    }
}

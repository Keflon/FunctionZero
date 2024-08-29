using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalisationZero.Localisation
{
    public class LocalisationItem
    {
        public LocalisationItem(string conditionExpression, string localisedText)
        {
            ConditionExpression = conditionExpression;
            LocalisedText = localisedText;
        }

        public string ConditionExpression { get; }
        public string LocalisedText { get; }
    }
}

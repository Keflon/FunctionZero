using FunctionZero.ExpressionParserZero.Binding;
using FunctionZero.ExpressionParserZero.Evaluator;
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
            CompiledExpression = ExpressionParserFactory.GetExpressionParser().Parse(conditionExpression);

            LocalisedText = localisedText;
        }

        public string ConditionExpression { get; }
        public ExpressionTree CompiledExpression { get; }
        public string LocalisedText { get; }
    }
}

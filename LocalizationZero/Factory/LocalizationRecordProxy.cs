using FunctionZero.ObjectGraphZero;
using LocalizationZero.Localization;

namespace LocalizationZero.Factory
{
    internal class LocalizationRecordProxy : IObjectConsumer
    {
        char[] comma = { ',' };
        public string[] Parameters { get; }
        public LocalizationRecordProxy(string id, string parameters)
        {
            Id = id;
            Parameters = parameters?.Split(comma, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? new string[0];
            Items = new List<LocalizationItem>();
        }

        public IList<LocalizationItem> Items { get; }
        public string Id { get; }

        public bool ConsumeObject(object o)
        {
            if (o is LocalizationItem item)
            {
                Items.Add(item);
                return true;
            }
            else
                throw new InvalidDataException($"LocalizationRecord cannot consume a {o.ToString()}");
        }
    }
}
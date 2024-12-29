using FunctionZero.ObjectGraphZero;
using LocalizationZero.Localization;

namespace LocalizationZero.Factory
{
    internal class LocalizationRecordProxy : IObjectConsumer
    {


        public LocalizationRecordProxy(string id)
        {
            Id = id;
            Items = new List<LocalizationItem>()
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
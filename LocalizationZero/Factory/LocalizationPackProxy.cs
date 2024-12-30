using FunctionZero.ObjectGraphZero;

namespace LocalizationZero.Factory
{
    internal class LocalizationPackProxy : IObjectConsumer
    {
        public LocalizationPackProxy(string language, string localizedName)
        {
            Language = language;
            LocalizedName = localizedName;
        }

        public string Language { get; }
        public string LocalizedName { get; }

        public bool ConsumeObject(object o)
        {
            //if (o is LocalizationItem item)
            //{
            //    Items.Add(item);
            //    return true;
            //}
            //else
            //    throw new InvalidDataException($"LocalizationRecord cannot consume a {o.ToString()}");

            return true;
        }
    }
}
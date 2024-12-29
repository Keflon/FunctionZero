using FunctionZero.ObjectGraphZero.Factory;
using LocalizationZero.Localization;

namespace LocalizationZero.Factory
{
    internal class LocalizationPackFactory : IObjectFactory
    {
        private readonly List<LocalizationRecord> _localizationRecordList;

        public object Result { get; private set; }

        public LocalizationPackFactory()
        {
            _localizationRecordList = new List<LocalizationRecord>();
        }

        public bool Create(FactoryData factoryData, out object createdObject)
        {
            switch (factoryData.Type)
            {
                case "LocalizationPack":
                    {
                        string language;
                        string friendlyName;
                        if (factoryData.Attributes.TryGetValue("Language", out language) == false)
                            throw new InvalidDataException($"Expected 'Language' attribute in 'LocalizationPack' element");

                        if (factoryData.Attributes.TryGetValue("FriendlyName", out friendlyName) == false)
                            throw new InvalidDataException($"Expected 'FriendlyName' attribute in 'LocalizationPack' element");

                        createdObject = new LocalizationPackProxy(language, friendlyName);
                        return true;
                    }
                case "Record":
                    {
                        string id;
                        if (factoryData.Attributes.TryGetValue("Id", out id) == false)
                            throw new InvalidDataException($"Expected 'Id' attribute in 'Record' element");

                        createdObject = new LocalizationRecordProxy(id);
                        return true;
                    }
                case "Item":
                    {
                        string condition;
                        if (factoryData.Attributes.TryGetValue("Condition", out condition) == false)
                            condition = "true";

                        string text;
                        if (factoryData.Attributes.TryGetValue("Text", out text) == false)
                            throw new InvalidDataException($"Expected 'Text' attribute in 'Item' element");


                        createdObject = new LocalizationItem(condition, text);
                        return true;
                    }
                default:
                    throw new InvalidOperationException($"Factory cannot create object type {factoryData.Type}");

            }
        }

        public void Created(object createdObject, string untrimmedContent, object parentObject)
        {
            if(createdObject is LocalizationPackProxy localizationPackProxy)
            {
                // Create a LocalizationPack from the list of LocalizationRecord instances and set the Result.

                if (Result != null)
                    throw new InvalidOperationException("There is more than one root LocalizationPack Element");

                (LocalizationPack LocalizationPack, string languageName) result = (new LocalizationPack(_localizationRecordList), localizationPackProxy.FriendlyName);
                Result = result;
            }
            else if (createdObject is LocalizationRecordProxy localizationRecordProxy)
            {
                // Create a LocalizationRecord and add it to a list.
                var record = new LocalizationRecord(localizationRecordProxy.Items);
                _localizationRecordList.Add(record);
            }
            else if(createdObject is LocalizationItem)
            {
                // Do nothing.
            }
            else
            {
                throw new InvalidDataException($"Unexpected createdObject {createdObject}");
            }
        }

        public void FoundTextFragment(object ownerObject, string untrimmedContent)
        {

        }

        public void ResetState()
        {
            Result = null;
        }
    }
}

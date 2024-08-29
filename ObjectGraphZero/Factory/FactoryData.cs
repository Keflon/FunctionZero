// Copyright 2016 FunctionZero Ltd.
// 
// 

namespace FunctionZero.ObjectGraphZero.Factory
{
	public class FactoryData
	{
		public ObjectDetails ObjectDetails { get; private set; }
		public string Type { get; private set; }
		public FactoryAttributes Attributes { get; private set; }
		public string NamespaceUri { get; private set; }

		public FactoryData(ObjectDetails objectDetails, string strType, FactoryAttributes attributes, string namespaceUri)
		{
			ObjectDetails = objectDetails;
			Type = strType;
			Attributes = attributes;
			NamespaceUri = namespaceUri;
		}
	}
}
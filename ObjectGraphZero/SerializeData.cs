// Copyright 2016 FunctionZero Ltd.
// 
// 

using System.Collections.Generic;
using FunctionZero.ObjectGraphZero.Factory;

namespace FunctionZero.ObjectGraphZero
{
	public class SerializeData
	{
		public SerializeData(string elementName, FactoryAttributes attributes, IEnumerable<ICanSerialize> components, string content)
		{
			ElementName = elementName;
			Attributes = attributes;
			Content = content;
			Components = components;
		}

		public string ElementName { get; }
		public FactoryAttributes Attributes { get; }
		public string Content { get; private set; }
		public IEnumerable<ICanSerialize> Components { get; }
	}
}
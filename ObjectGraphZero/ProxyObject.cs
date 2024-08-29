// Copyright 2016 FunctionZero Ltd.
// 
// 

using System.Collections.Generic;
using FunctionZero.ObjectGraphZero.Factory;

namespace FunctionZero.ObjectGraphZero
{
	public class ProxyObject : IObjectConsumer, ICanSerialize, IContentContainer
	{
		public string ElementName { get; }
		public FactoryAttributes Attributes { get; }
		public string Content { get; set; }

		public ProxyObject(string elementName, FactoryAttributes attributes, string content)
		{
			ElementName = elementName;
			Attributes = attributes;
			Content = content;
			Children = new List<ICanSerialize>();
		}

		public List<ICanSerialize> Children { get; }

		public bool ConsumeObject(object o)
		{
			var child = o as ICanSerialize;
			if(child != null)
			{
				this.Children.Add(child);
				return true;
			}
			return false;
		}

		public bool IsDirty { get; set; }

		public SerializeData GetSerializeData(object args)
		{
			return new SerializeData(ElementName, Attributes, Children, SerialiseContent());
		}

		public string SerialiseContent()
		{
			// HACK
			if(string.IsNullOrEmpty(Content))
				return null;

			return Content;
		}

		public void DeserialiseContent(string content)
		{
			Content = content;
		}
	}
}

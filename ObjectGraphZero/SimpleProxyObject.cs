// Copyright 2016 FunctionZero Ltd.
// 
// 

using System.Collections.Generic;
using FunctionZero.ObjectGraphZero.Factory;

namespace FunctionZero.ObjectGraphZero
{
	public class SimpleProxyObject : IObjectConsumer
	{
		public string ElementName { get; }
		public FactoryAttributes Attributes { get; }
		public string Content { get; set; }

		public SimpleProxyObject(string elementName, FactoryAttributes attributes, string content)
		{
			ElementName = elementName;
			Attributes = attributes;
			Content = content;
			Children = new List<object>();
		}

		public List<object> Children { get; }

		public bool ConsumeObject(object o)
		{
			var child = o;
			if(child != null)
			{
				this.Children.Add(child);
				return true;
			}
			return false;
		}
	}
}

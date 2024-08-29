// Copyright 2016 FunctionZero Ltd.
// 
// 

using System;
using System.Collections.Generic;
using System.Xml;

namespace FunctionZero.ObjectGraphZero
{
	public static class XmlSerializer
	{
		public static bool SerializeToXml(ICanSerialize obj, XmlWriter xw, Predicate<ICanSerialize> canSerialize, object serializeParam, List<ICanSerialize> serializedObjects, bool useTextFragments = false)
		{
			if(canSerialize(obj))
			{
				if(serializedObjects != null)
					serializedObjects.Add(obj);

				SerializeData xd = obj.GetSerializeData(serializeParam);

				if(xd != null)
				{
					if(xd.ElementName != null)
					{
						xw.WriteStartElement(xd.ElementName);

						if(xd.Attributes != null)
						{
							foreach(var attribute in xd.Attributes)
							{
								xw.WriteAttributeString(attribute.Key, attribute.Value);
							}
						}
						if(useTextFragments == false)
							xw.WriteString(xd.Content);
					}
					else if(useTextFragments)
					{
						xw.WriteString(xd.Content);
						return true;
					}
					if(xd.Components != null)
					{
						foreach(var child in xd.Components)
						{
							SerializeToXml(child, xw, canSerialize, serializeParam, serializedObjects, useTextFragments);
						}
					}
					if(xd.ElementName != null)
						xw.WriteEndElement();

					return true;
				}
				return false;
			}
			return false;
		}
	}
}
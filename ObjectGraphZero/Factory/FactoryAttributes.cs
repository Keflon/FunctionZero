// Copyright 2016 FunctionZero Ltd.
// 
// 

using System.Collections.Generic;

namespace FunctionZero.ObjectGraphZero.Factory
{
	public class FactoryAttributes : Dictionary<string, string>
	{
		public string GetAttribute(string attributeName, string strDefault)
		{
			string strAttributeValue;
			return this.TryGetValue(attributeName, out strAttributeValue) ? strAttributeValue : strDefault;
		}

		public string GetAttribute(string attributeName)
		{
			return this[attributeName];
		}

		public long GetLong(string attributeName, long longDefault)
		{
			string strAttributeValue;
			if(this.TryGetValue(attributeName, out strAttributeValue) == true)
			{
				long retVal;
				if(long.TryParse(strAttributeValue, out retVal) == true)
					return retVal;
			}
			return longDefault;
		}

		public long GetLong(string attributeName)
		{
			return long.Parse(this[attributeName]);
		}


		public FactoryAttributes Copy()
		{
			FactoryAttributes retVal = new FactoryAttributes();

			// This works because strings are immutable. We therefore just need to copy the dictionary.
			// Note: Can I just somehow use this.MemberwiseClone();??

			foreach(KeyValuePair<string, string> kvp in this)
			{
				retVal.Add(kvp.Key, kvp.Value);
			}

			return retVal;
		}

		public bool GetBool(string attributeName, bool boolDefault)
		{
			string strAttributeValue;
			if(this.TryGetValue(attributeName, out strAttributeValue) == true)
			{
				bool retVal;
				if(bool.TryParse(strAttributeValue, out retVal) == true)
					return retVal;
			}
			return boolDefault;
		}

		public bool GetBool(string attributeName)
		{
			return bool.Parse(this[attributeName]);
		}
	}
}
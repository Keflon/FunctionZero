// Copyright 2016 FunctionZero Ltd.
// 
// 

using System;

namespace FunctionZero.ObjectGraphZero
{
	public class TextFragment : ICanSerialize
	{
		public string UntrimmedText { get; private set; }

		public TextFragment(string untrimmedText)
		{
			UntrimmedText = untrimmedText;
		}

		public bool IsDirty
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public SerializeData GetSerializeData(object args)
		{
			return new SerializeData(null, null, null, UntrimmedText);
		}
	}
}

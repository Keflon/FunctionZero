// Copyright 2016 FunctionZero Ltd.
// 
// 

namespace FunctionZero.ObjectGraphZero
{
	public interface ICanSerialize
	{
		bool IsDirty { get; set; }
		SerializeData GetSerializeData(object args);
	}
}
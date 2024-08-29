// Copyright 2016 FunctionZero Ltd.
// 
// 
namespace FunctionZero.ObjectGraphZero
{
	public interface IGraphObject
	{
		// Each graph object must be uniquely identifiable.
		long Guid { get; }

		// In future I might add a 'name' property etc.
	}
}
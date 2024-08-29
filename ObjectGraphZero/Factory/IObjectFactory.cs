// Copyright 2016 FunctionZero Ltd.
// 
// 

namespace FunctionZero.ObjectGraphZero.Factory
{
	public interface IObjectFactory
	{
		// TODO: Add a namespace string in here?
		// string Namespace { get; }
		bool Create(FactoryData factoryData, out object createdObject);

		// TODO: untrimmedContent should be a StringBuilder or perhaps a stream.
		void Created(object createdObject, string untrimmedContent, object parentObject);

		void FoundTextFragment(object ownerObject, string untrimmedContent);
		void ResetState();

		object Result { get; }
	}
}

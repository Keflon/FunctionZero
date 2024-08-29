// Copyright 2016 FunctionZero Ltd.
// 
// 
namespace FunctionZero.ObjectGraphZero
{
	public interface IContentContainer
	{
		string SerialiseContent();						// Called when serialising. It doesn't matter what the content is as far as the object is concerned. As long as it's passed to the serialiser as a string.
		void DeserialiseContent(string content);		// Called when deserialising. The deserialiser passes the 'content' as a string to the object which can convert it to whatever it sees fit.
	}
}


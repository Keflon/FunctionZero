// Copyright 2016 FunctionZero Ltd.
// 
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using FunctionZero.ObjectGraphZero.Factory;

namespace FunctionZero.ObjectGraphZero
{
	public class ObjectDetails
	{
		public IObjectFactory Factory { get; private set; }
		public object FactoryObject { get; private set; }
		public string Path { get; private set; }
		public StringBuilder ContentBuilder { get; private set; }

		public ObjectDetails(IObjectFactory factory, object factoryObject, string path, StringBuilder contentBuilder)
		{
			Factory = factory;
			FactoryObject = factoryObject;
			Path = path;
			ContentBuilder = contentBuilder;
		}
	}



	public static class XmlDeserializer
	{
		/// <summary>
		/// Single factory version of BuildObjectGraphFromXml(XmlReader reader, ObjectFactoryDictionary objectFactoryList, Action<object, string> objectCreatedAction = null)
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectFactory"></param>
		/// <param name="objectCreatedAction"></param>
		/// <returns>The objectFactory that was passed in.</returns>
		public static IObjectFactory BuildObjectGraphFromXml(XmlReader reader, IObjectFactory objectFactory, Action<object, string> objectCreatedAction = null)
		{
			ObjectFactoryDictionary factoryList = new ObjectFactoryDictionary();
			factoryList.Add("", objectFactory);
			BuildObjectGraphFromXml(reader, factoryList, objectCreatedAction);
			return objectFactory;
		}

		/// <summary>
		/// This method parses an Xml document and creates objects from XmlElement entities and their attributes. Xml text content is collected and provided to any created object.
		/// </summary>
		/// <param name="reader"> The wrapper for the XmlDocument </param>
		/// <param name="objectFactoryList"> A factory per namespace to use to create each object based on the XmlElement name and a dictionary formed from the attributes it contains. </param>
		/// <param name="objectCreatedAction">An Action to perform after an object has been created.</param>
		/// <returns> The list of factories that were passed in, from each of which you can get a result object. </returns>
		public static ObjectFactoryDictionary BuildObjectGraphFromXml(XmlReader reader, ObjectFactoryDictionary objectFactoryList, Action<object, string> objectCreatedAction = null)
		{
			bool firstElementHasBeenClosed = false;		// Only parse the root node - allows xml fragments to be parsed.
			Stack<ObjectDetails> builderStack = new Stack<ObjectDetails>();

			builderStack.Push(new ObjectDetails(null, null, null, null));

			object rootObject = null;

			// Parse the file and build the object graph.
			while(!firstElementHasBeenClosed)
			{
				if(reader.Read() == false)
					throw new EndOfStreamException();

				switch(reader.NodeType)
				{
					#region NeverHitByMe

					case XmlNodeType.Attribute: // Never hit even if attributes are present. Parse them seperately.
						Debug.WriteLine("Attribute : Name :'" + reader.Name + "' Value:'" + reader.Value);
						break;
					case XmlNodeType.EndEntity:
						Debug.WriteLine("EndEntity");
						break;
					case XmlNodeType.Entity:
						Debug.WriteLine("Entity");
						break;
					case XmlNodeType.EntityReference:
						Debug.WriteLine("EntityReference");
						break;
					case XmlNodeType.None:
						Debug.WriteLine("None");
						break;
					case XmlNodeType.Notation:
						Debug.WriteLine("Notation");
						break;
					case XmlNodeType.ProcessingInstruction:
						Debug.WriteLine("ProcessingInstruction");
						break;
					case XmlNodeType.SignificantWhitespace:
						Debug.WriteLine("SignificantWhitespace");
						break;
					case XmlNodeType.Document:
						Debug.WriteLine("Document");
						break;
					case XmlNodeType.DocumentFragment:
						Debug.WriteLine("DocumentFragment");
						break;
					case XmlNodeType.DocumentType:
						Debug.WriteLine("DocumentType");
						break;

					#endregion NeverHitByMe

					#region IrrelevantToMe

					case XmlNodeType.Comment:
						//Debug.WriteLine("Comment: '" + reader.Value + "'");
						break;
					case XmlNodeType.Whitespace:
						//	Debug.WriteLine("Whitespace");
						break;
					case XmlNodeType.XmlDeclaration:
						//Debug.WriteLine("XmlDeclaration");
						break;
					default:
						break;

					#endregion

					case XmlNodeType.Element:
						{
							ObjectDetails currentParentObjectDetails = builderStack.Peek();
#if XML_GRAPH_DEBUG
							Debug.WriteLine("Creating " + currentPath + reader.Name);
#endif
							// Use the current namespace to determine which factory to use to create the object ...
							// TODO: Consider if factory1 fails to create the object then look up the objectStack for a factory that 
							// TODO: can create it, before then trying factory2.

							// Create dictionary and factory data from attributes. Create object from factory.
							FactoryAttributes attributes = GetAttributes(reader);
							FactoryData factoryData = new FactoryData(currentParentObjectDetails, reader.Name, attributes, reader.NamespaceURI);

							object currentObject = null;

							bool isCreated = false;
							// If the current parent object has (is) an object factory then use it.
							IObjectFactory factory = currentParentObjectDetails.FactoryObject as IObjectFactory;
							if(factory != null)
								isCreated = factory.Create(factoryData, out currentObject); // May be null.

							if(isCreated == false)
							{
								// DayBook creates everything here:
								objectFactoryList.TryGetValue(reader.NamespaceURI, out factory);
								if(factory != null)
									isCreated = factory.Create(factoryData, out currentObject); // currentObject may be null when isCreated is true.
								Debug.Assert(factory != null);

								if(isCreated == false)
								{
									// If we use this, tell Phil because his code can't skip.
									Debug.Assert(false, "If we use this, tell Phil because his code can't skip.");
									reader.Skip(); // Is this right? Should we instead goto case XmlNodeType.EndElement ??? PRETTY SURE NOT.
									break;
								}
							}

							Debug.Assert(isCreated == true, "If IsCreated is false we should have bailed out by now!");

							StringBuilder currentStringBuilder = null;

							if(currentObject != null)
							{
								if(rootObject == null) rootObject = currentObject;
								// TODO: Always have a stringbuilder?
								currentStringBuilder = new StringBuilder();
							}
							// Push the new object and its entourage onto the stack ...
							string objectPath = currentParentObjectDetails.Path + reader.Name + "/";
							builderStack.Push(new ObjectDetails(factory, currentObject, objectPath, currentStringBuilder));

							if(objectCreatedAction != null)
								objectCreatedAction(currentObject, objectPath);     // currentObject may be null.

							if(reader.IsEmptyElement)
								goto case XmlNodeType.EndElement;

							break;
						}
					case XmlNodeType.EndElement:
						{
							// Pop the completed object from stack and let it interact with any parent object.
							// Note the first stacked element will be null.
#if XML_GRAPH_DEBUG
						Debug.WriteLine("EndElement. Name: '" + reader.Name + "'");
#endif
							ObjectDetails lastObjectDetails = builderStack.Pop();
							Debug.Assert(lastObjectDetails.Factory != null, "lastObjectDetails.Factory is null");
							ObjectDetails lastObjectParentDetails = builderStack.Peek();
							string untrimmedContent = lastObjectDetails.ContentBuilder == null ? null : lastObjectDetails.ContentBuilder.ToString();
							lastObjectDetails.Factory.Created(lastObjectDetails.FactoryObject, untrimmedContent, lastObjectParentDetails.FactoryObject);

							if(builderStack.Count == 1)
								firstElementHasBeenClosed = true;
						}
						break;

					case XmlNodeType.Text:
						{
							var lastObjectDetails = builderStack.Peek();
							lastObjectDetails.Factory.FoundTextFragment(lastObjectDetails.FactoryObject, reader.Value);
							if(lastObjectDetails.ContentBuilder != null)
								lastObjectDetails.ContentBuilder.Append(reader.Value);
						}
						break;
					// TODO: Should CDATA be different?
					case XmlNodeType.CDATA:
						{
							Debug.WriteLine("CDATA");
							var lastObjectDetails = builderStack.Peek();

							if(lastObjectDetails.ContentBuilder != null)
								lastObjectDetails.ContentBuilder.Append(reader.Value);
						}
						break;
				}

			}
			builderStack.Pop();// Pop the null that was added at the start.
			Debug.Assert(builderStack.Count == 0);

			return objectFactoryList;
		}

		private static FactoryAttributes GetAttributes(XmlReader reader)
		{
			FactoryAttributes attributes = new FactoryAttributes();
			if(reader.HasAttributes)
			{
#if XML_GRAPH_DEBUG
				StringBuilder sb = new StringBuilder();
				sb.Append("Attributes of <" + reader.Name + ">");
#endif
				while(reader.MoveToNextAttribute())
				{
#if XML_GRAPH_DEBUG
					sb.Append(string.Format(" {0}={1}   ", reader.Name, reader.Value));
#endif
					attributes.Add(reader.Name, reader.Value);
				}
				// Move the reader back to the element node.
				reader.MoveToElement();

#if XML_GRAPH_DEBUG
				Debug.WriteLine(sb.ToString());
#endif
			}
			return attributes;
		}
	}
}
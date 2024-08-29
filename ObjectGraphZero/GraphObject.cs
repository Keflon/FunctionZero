// Copyright 2016 FunctionZero Ltd.
// 
// 

namespace FunctionZero.ObjectGraphZero
{
	public class GraphObject : IGraphObject
	{
		private static long _lastGuid = 0;
		private readonly long _guid;

		public GraphObject()
		{
			_guid = GetNextGuid();
		}

		#region IGraphObject Members

		public long Guid
		{
			get { return _guid; }
		}

		#endregion

		private static long GetNextGuid()
		{
			return ++_lastGuid;
		}
	}
}
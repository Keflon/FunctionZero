using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionZero.TreeZero.Exceptions
{
    public enum ExceptionReason
    {
        Unknown = 0,
        ParentToSelf,
        ParentToDescendant,
        CollectionItemAlreadyParented,
        ChildAddedToSameParent
    }
    public class TreeZeroException : Exception
    {
        public TreeZeroException(object node, ExceptionReason reason, string message = null) : base(message ?? reason.ToString())
        {
            Node = node;
            Reason = reason;
        }

        public object Node { get; }
        public ExceptionReason Reason { get; }
    }
}

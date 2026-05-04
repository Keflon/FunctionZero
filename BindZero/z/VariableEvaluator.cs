using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Operands;
using System.Collections.Generic;
using System.Reflection;

namespace FunctionZero.Maui.zBind.z
{
    internal class VariableEvaluator : IBackingStore
    {
        private object[] _values;
        private readonly IList<string> _keys;
        private readonly Bind _bindingExtension;

        // Cache for reflection results to improve performance
        // Key: (Type, PropertyName), Value: PropertyInfo
        private readonly Dictionary<(Type, string), PropertyInfo> _propertyCache = new Dictionary<(Type, string), PropertyInfo>();

        public VariableEvaluator(IList<string> keys, Bind bindingExtension)
        {
            _keys = keys;
            _bindingExtension = bindingExtension;
        }

        internal void SetValues(object[] values)
        {
            _values = values;
        }

        public (OperandType type, object value) GetValue(string qualifiedName)
        {

            int index = _keys.IndexOf(qualifiedName);
            object value = _values[index];

            if (value == null)
                return (OperandType.Null, null);

            if (BackingStoreHelpers.OperandTypeLookup.TryGetValue(value.GetType(), out var theOperandType))
                return (theOperandType, value);

            return (OperandType.Object, value);
        }

        private static readonly char[] _dot = new[] { '.' };

        /// <summary>
        /// Gets a cached PropertyInfo for the specified type and property name.
        /// </summary>
        private PropertyInfo GetCachedPropertyInfo(Type type, string propertyName)
        {
            var key = (type, propertyName);

            if (!_propertyCache.TryGetValue(key, out var propInfo))
            {
                propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                _propertyCache[key] = propInfo;
            }

            return propInfo;
        }

        public void SetValue(string qualifiedName, object value)
        {
            var host = _bindingExtension.Source ?? _bindingExtension.BindableTarget?.BindingContext;
            if (host != null)
            {
                var bits = qualifiedName.Split(_dot);

                for (int c = 0; c < bits.Length - 1; c++)
                {
                    PropertyInfo prop = GetCachedPropertyInfo(host.GetType(), bits[c]);
                    if (null != prop && prop.CanRead)
                    {
                        host = prop.GetValue(host);
                        if (host == null)
                            return;
                    }
                    else
                        return;
                }
                var variableName = bits[bits.Length - 1];

                PropertyInfo prop2 = GetCachedPropertyInfo(host.GetType(), variableName);
                if (null != prop2 && prop2.CanWrite)
                {
                    prop2.SetValue(host, value, null);
                }
            }
        }

        public void SetValue(PropertyInfo propInfo, object value)
        {
            var host = _bindingExtension.Source ?? _bindingExtension.BindableTarget?.BindingContext;
            if (host != null)
            {
                propInfo.SetValue(host, value, null);
            }
        }

        public PropertyInfo GetPropertyInfo(string qualifiedName)
        {
            var host = _bindingExtension.Source ?? _bindingExtension.BindableTarget?.BindingContext;
            if (host != null)
            {
                var bits = qualifiedName.Split(_dot);

                for (int c = 0; c < bits.Length - 1; c++)
                {
                    PropertyInfo prop = GetCachedPropertyInfo(host.GetType(), bits[c]);
                    if (null != prop && prop.CanRead)
                    {
                        host = prop.GetValue(host);
                        if (host == null)
                            return null;
                    }
                    else
                        return null;
                }
                var variableName = bits[bits.Length - 1];

                return GetCachedPropertyInfo(host.GetType(), variableName);
            }
            return null;
        }
    }
}
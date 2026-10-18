using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Operands;
using System.Collections.Generic;
using System.Reflection;

namespace FunctionZero.Maui.zBind.z
{
    internal class VariableEvaluator : IBackingStore
    {
        private object[]? _values;
        private readonly IList<string> _keys;
        private readonly Bind _bindingExtension;

        // Cache for reflection results to improve performance
        // Key: (Type, PropertyName), Value: PropertyInfo
        private readonly Dictionary<(Type, string), PropertyInfo?> _propertyCache = new();

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
            if (index < 0)
                throw new KeyNotFoundException($"The expression variable '{qualifiedName}' has no corresponding binding.");
            if (_values == null)
                throw new InvalidOperationException("Binding values have not been supplied to the expression evaluator.");

            object value = _values[index];

            if (value == null)
                return (OperandType.Null, null!);

            if (BackingStoreHelpers.OperandTypeLookup.TryGetValue(value.GetType(), out var theOperandType))
                return (theOperandType, value);

            return (OperandType.Object, value);
        }

        private static readonly char[] _dot = ['.'];

        /// <summary>
        /// Gets a cached PropertyInfo for the specified type and property name.
        /// </summary>
        private PropertyInfo? GetCachedPropertyInfo(Type type, string propertyName)
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
            if (TryGetPropertyTarget(qualifiedName, out var host, out var propertyInfo) && propertyInfo is { CanWrite: true })
            {
                propertyInfo.SetValue(host, value, null);
            }
        }

        public PropertyInfo? GetPropertyInfo(string qualifiedName)
        {
            return TryGetPropertyTarget(qualifiedName, out _, out var propertyInfo) ? propertyInfo : null;
        }

        private bool TryGetPropertyTarget(string qualifiedName, out object? host, out PropertyInfo? propertyInfo)
        {
            var currentHost = _bindingExtension.Source ?? _bindingExtension.BindableTarget?.BindingContext;
            host = null;
            propertyInfo = null;

            if (currentHost != null)
            {
                var bits = qualifiedName.Split(_dot);

                for (int c = 0; c < bits.Length - 1; c++)
                {
                    PropertyInfo? prop = GetCachedPropertyInfo(currentHost.GetType(), bits[c]);
                    if (null != prop && prop.CanRead)
                    {
                        currentHost = prop.GetValue(currentHost);
                        if (currentHost == null)
                            return false;
                    }
                    else
                        return false;
                }
                var variableName = bits[bits.Length - 1];

                propertyInfo = GetCachedPropertyInfo(currentHost.GetType(), variableName);
                if (propertyInfo != null)
                {
                    host = currentHost;
                    return true;
                }
            }

            return false;
        }
    }
}
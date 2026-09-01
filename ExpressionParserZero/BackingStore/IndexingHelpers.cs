using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.Operands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace FunctionZero.ExpressionParserZero.BackingStore
{
    internal static class IndexingHelpers
    {
        public static IndexedReferenceOperand CreateReference(object target, object[] indices, long parserPosition)
        {
            if (target == null)
                throw Error(parserPosition, "Cannot index a null value.");
            if (indices == null || indices.Length == 0)
                throw Error(parserPosition, "At least one index is required.");

            if (target is Array array)
                return CreateArrayReference(array, indices, parserPosition);

            var targetType = target.GetType();
            var dictionaryInterface = FindGenericInterface(targetType, typeof(IDictionary<,>));
            if (dictionaryInterface != null)
                return CreateGenericDictionaryReference(target, dictionaryInterface, indices, parserPosition);

            if (target is IDictionary dictionary)
                return CreateDictionaryReference(dictionary, indices, parserPosition);

            if (target is IList list)
                return CreateListReference(list, targetType, indices, parserPosition);

            var listInterface = FindGenericInterface(targetType, typeof(IList<>));
            if (listInterface != null)
                return CreateGenericListReference(target, listInterface, indices, parserPosition);

            throw Error(parserPosition, $"Values of type '{targetType.FullName}' do not support indexing.");
        }

        private static IndexedReferenceOperand CreateArrayReference(Array array, object[] indices, long parserPosition)
        {
            if (indices.Length != array.Rank)
                throw Error(parserPosition, $"Array rank is {array.Rank}, but {indices.Length} indices were supplied.");

            var convertedIndices = new int[indices.Length];
            for (var index = 0; index < indices.Length; index++)
                convertedIndices[index] = ConvertIndex(indices[index], parserPosition);

            var elementType = array.GetType().GetElementType();
            return CreateOperand(
                parserPosition,
                elementType,
                () => Read(parserPosition, () => array.GetValue(convertedIndices)),
                value => Write(parserPosition, () => array.SetValue(ConvertValue(value, elementType, parserPosition), convertedIndices)));
        }

        private static IndexedReferenceOperand CreateListReference(IList list, Type targetType, object[] indices, long parserPosition)
        {
            RequireSingleIndex(indices, parserPosition);
            var index = ConvertIndex(indices[0], parserPosition);
            var listInterface = FindGenericInterface(targetType, typeof(IList<>));
            var elementType = listInterface?.GetGenericArguments()[0] ?? typeof(object);

            return CreateOperand(
                parserPosition,
                elementType,
                () => Read(parserPosition, () => list[index]),
                value =>
                {
                    if (list.IsReadOnly)
                        throw Error(parserPosition, "The indexed list is read-only.");
                    Write(parserPosition, () => list[index] = ConvertValue(value, elementType, parserPosition));
                });
        }

        private static IndexedReferenceOperand CreateGenericListReference(object target, Type listInterface, object[] indices, long parserPosition)
        {
            RequireSingleIndex(indices, parserPosition);
            var index = ConvertIndex(indices[0], parserPosition);
            var elementType = listInterface.GetGenericArguments()[0];
            var itemProperty = listInterface.GetProperty("Item");
            var collectionInterface = typeof(ICollection<>).MakeGenericType(elementType);
            var isReadOnlyProperty = collectionInterface.GetProperty("IsReadOnly");

            return CreateOperand(
                parserPosition,
                elementType,
                () => Read(parserPosition, () => itemProperty.GetValue(target, new object[] { index })),
                value =>
                {
                    if ((bool)isReadOnlyProperty.GetValue(target))
                        throw Error(parserPosition, "The indexed list is read-only.");
                    Write(parserPosition, () => itemProperty.SetValue(target, ConvertValue(value, elementType, parserPosition), new object[] { index }));
                });
        }

        private static IndexedReferenceOperand CreateGenericDictionaryReference(object target, Type dictionaryInterface, object[] indices, long parserPosition)
        {
            RequireSingleIndex(indices, parserPosition);
            var genericArguments = dictionaryInterface.GetGenericArguments();
            var keyType = genericArguments[0];
            var valueType = genericArguments[1];
            var key = ConvertValue(indices[0], keyType, parserPosition);
            var itemProperty = dictionaryInterface.GetProperty("Item");
            var pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
            var collectionInterface = typeof(ICollection<>).MakeGenericType(pairType);
            var isReadOnlyProperty = collectionInterface.GetProperty("IsReadOnly");

            return CreateOperand(
                parserPosition,
                valueType,
                () => Read(parserPosition, () => itemProperty.GetValue(target, new[] { key })),
                value =>
                {
                    if ((bool)isReadOnlyProperty.GetValue(target))
                        throw Error(parserPosition, "The indexed dictionary is read-only.");
                    Write(parserPosition, () => itemProperty.SetValue(target, ConvertValue(value, valueType, parserPosition), new[] { key }));
                },
                false);
        }

        private static IndexedReferenceOperand CreateDictionaryReference(IDictionary dictionary, object[] indices, long parserPosition)
        {
            RequireSingleIndex(indices, parserPosition);
            var key = indices[0];
            return CreateOperand(
                parserPosition,
                typeof(object),
                () => Read(parserPosition, () => dictionary[key]),
                value =>
                {
                    if (dictionary.IsReadOnly)
                        throw Error(parserPosition, "The indexed dictionary is read-only.");
                    Write(parserPosition, () => dictionary[key] = value);
                },
                false);
        }

        private static IndexedReferenceOperand CreateOperand(long parserPosition, Type declaredType, Func<object> getter, Action<object> setter, bool inspectCurrentValue = true)
        {
            object currentValue = null;
            if (inspectCurrentValue)
            {
                try
                {
                    currentValue = getter();
                }
                catch (ExpressionEvaluatorException)
                {
                    throw;
                }
            }

            var operandType = GetOperandType(declaredType, currentValue);
            return new IndexedReferenceOperand(parserPosition, operandType, getter, setter);
        }

        private static OperandType GetOperandType(Type declaredType, object value)
        {
            if (declaredType != null && BackingStoreHelpers.OperandTypeLookup.TryGetValue(declaredType, out var declaredOperandType))
                return declaredOperandType;
            if (value != null && BackingStoreHelpers.OperandTypeLookup.TryGetValue(value.GetType(), out var valueOperandType))
                return valueOperandType;
            return value == null ? OperandType.Null : OperandType.Object;
        }

        private static Type FindGenericInterface(Type type, Type genericTypeDefinition)
        {
            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                return type;

            foreach (var item in type.GetInterfaces())
            {
                if (item.IsGenericType && item.GetGenericTypeDefinition() == genericTypeDefinition)
                    return item;
            }

            return null;
        }

        private static void RequireSingleIndex(object[] indices, long parserPosition)
        {
            if (indices.Length != 1)
                throw Error(parserPosition, $"This value requires one index, but {indices.Length} indices were supplied.");
        }

        private static int ConvertIndex(object value, long parserPosition)
        {
            try
            {
                var converted = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                var numericValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (numericValue != converted)
                    throw new InvalidCastException();
                return converted;
            }
            catch (Exception)
            {
                throw Error(parserPosition, $"Index '{value ?? "null"}' is not an integer.");
            }
        }

        private static object ConvertValue(object value, Type targetType, long parserPosition)
        {
            if (value == null)
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    throw Error(parserPosition, $"Null cannot be assigned to '{targetType.FullName}'.");
                return null;
            }

            if (targetType.IsInstanceOfType(value))
                return value;

            var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            try
            {
                if (conversionType.IsEnum)
                    return value is string text ? Enum.Parse(conversionType, text, true) : Enum.ToObject(conversionType, value);
                return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw Error(parserPosition, $"Value of type '{value.GetType().FullName}' cannot be converted to '{targetType.FullName}'.");
            }
        }

        private static object Read(long parserPosition, Func<object> action)
        {
            try
            {
                return action();
            }
            catch (ExpressionEvaluatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw Error(parserPosition, ex.InnerException?.Message ?? ex.Message);
            }
        }

        private static void Write(long parserPosition, Action action)
        {
            try
            {
                action();
            }
            catch (ExpressionEvaluatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw Error(parserPosition, ex.InnerException?.Message ?? ex.Message);
            }
        }

        private static ExpressionEvaluatorException Error(long parserPosition, string message)
        {
            return new ExpressionEvaluatorException(parserPosition, ExpressionEvaluatorException.ExceptionCause.BadOperand, message);
        }
    }
}
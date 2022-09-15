using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using BearMarkupLanguage.Serialization;
using SerializableAttribute = BearMarkupLanguage.Serialization.SerializableAttribute;

namespace BearMarkupLanguage.Helpers;

internal static class TypeExtension
{
    internal static Type GetGenericTypeDefinitionIfHas(this Type type)
    {
        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition();
        }
        else
        {
            return type;
        }
    }

    internal static Type GetStaticTupleType(this Type tupleType)
    {
        if (tupleType.GetInterface("ITuple") == null)
            throw new ArgumentException();

        var tupleTypeDef = tupleType.GetGenericTypeDefinition();

        if (tupleTypeDef == typeof(Tuple<>)
            || tupleTypeDef == typeof(Tuple<,>)
            || tupleTypeDef == typeof(Tuple<,,>)
            || tupleTypeDef == typeof(Tuple<,,,>)
            || tupleTypeDef == typeof(Tuple<,,,,>)
            || tupleTypeDef == typeof(Tuple<,,,,,>)
            || tupleTypeDef == typeof(Tuple<,,,,,,>)
            || tupleTypeDef == typeof(Tuple<,,,,,,,>))
        {
            return typeof(Tuple);
        }
        else if (tupleTypeDef == typeof(ValueTuple<>)
            || tupleTypeDef == typeof(ValueTuple<,>)
            || tupleTypeDef == typeof(ValueTuple<,,>)
            || tupleTypeDef == typeof(ValueTuple<,,,>)
            || tupleTypeDef == typeof(ValueTuple<,,,,>)
            || tupleTypeDef == typeof(ValueTuple<,,,,,>)
            || tupleTypeDef == typeof(ValueTuple<,,,,,,>)
            || tupleTypeDef == typeof(ValueTuple<,,,,,,,>))
        {
            return typeof(ValueTuple);
        }
        else
        {
            throw new ArgumentException();
        }
    }

    internal static bool HasInterface(this Type type, Type iType)
    {
        return type.GetInterfaces().Contains(iType);
    }

    internal static bool IsTupleType(this Type type)
    {
        return type.HasInterface(typeof(ITuple));
    }

    internal static bool IsListType(this Type type)
    {
        return type.HasInterface(typeof(IList));
    }

    internal static bool IsDictionaryType(this Type type)
    {
        return type.HasInterface(typeof(IDictionary));
    }

    internal static bool IsSerializableObject(this Type type)
    {
        foreach (var attr in type.GetCustomAttributes())
        {
            if (attr is SerializableAttribute)
                return true;
        }

        return false;
    }

    internal static bool HasIgnoreAttribute(this PropertyInfo prop)
    {
        foreach (var attr in Attribute.GetCustomAttributes(prop))
        {
            if (attr is IgnoreSerializationAttribute)
                return true;
        }

        return false;
    }

    internal static bool HasIgnoreAttribute(this FieldInfo field)
    {
        foreach (var attr in Attribute.GetCustomAttributes(field))
        {
            if (attr is IgnoreSerializationAttribute)
                return true;
        }

        return false;
    }
}

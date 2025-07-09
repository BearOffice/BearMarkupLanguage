using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Elements;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Helpers;
using System.Runtime.CompilerServices;

namespace BearMarkupLanguage.Serialization;

internal static class ElementConverter
{
    internal static IBaseElement BuildElement(object source, IConversionProvider[] providers)
    {
        if (source is null)
            return BuildEmptyElement(source, providers);

        var sourceType = source.GetType();

        // Build basic element for provider rule.
        if (TypeConverter.TryGetProvider(sourceType, providers, out _))
            return BuildBasicElement(source, providers);

        var targetType = IBaseElement.PreferredElementType(sourceType, providers);

        if (targetType == typeof(BasicElement))
            return BuildBasicElement(source, providers);
        else if (targetType == typeof(ListElement))
            return BuildListElement(source, providers);
        else if (targetType == typeof(DictionaryElement))
            return BuildDictionaryElement(source, providers);
        else
            throw new NotImplementedException();
    }

    private static BasicElement BuildBasicElement(object source, IConversionProvider[] providers)
    {
        var literal = TypeConverter.ConvertToLiteral(source, providers);

        return new BasicElement(literal);
    }

    private static EmptyElement BuildEmptyElement(object source, IConversionProvider[] providers)
    {
        return new EmptyElement();
    }

    private static ListElement BuildListElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();

        if (sourceType.IsListType())
        {
            if (sourceType.IsSZArray)
                return BuildArrayTypeElement(source, providers);
            else
                return BuildListTypeElement(source, providers);
        }
        else if (sourceType.IsTupleType())
        {
            return BuildTupleTypeElement(source, providers);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static ListElement BuildArrayTypeElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();
        var count = (int)sourceType.GetProperty("Length").GetValue(source, null);

        var tempArr = Array.CreateInstance(typeof(IBaseElement), count);

        for (var i = 0; i < count; i++)
        {
            var item = sourceType.GetMethods()
                                 .First(item => item.Name == "GetValue"
                                        && item.GetParameters().Length == 1
                                        && item.GetParameters()[0].ParameterType == typeof(int))
                                 .Invoke(source, new object[] { i });
            tempArr.SetValue(BuildElement(item, providers), i);
        }

        var elemsList = ((IBaseElement[])tempArr).ToList();

        return new ListElement(elemsList);
    }

    private static ListElement BuildListTypeElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();
        var count = (int)sourceType.GetProperty("Count").GetValue(source, null);

        var tempArr = Array.CreateInstance(typeof(IBaseElement), count);

        for (var i = 0; i < count; i++)
        {
            var item = sourceType.GetProperty("Item").GetValue(source, new object[] { i });
            tempArr.SetValue(BuildElement(item, providers), i);
        }

        var elemsList = ((IBaseElement[])tempArr).ToList();

        return new ListElement(elemsList);
    }

    private static ListElement BuildTupleTypeElement(object source, IConversionProvider[] providers)
    {
        //var sourceType = source.GetType();
        //var argumentTypes = sourceType.GetGenericArguments();

        var tuple = (ITuple)source;
        var tempArr = Array.CreateInstance(typeof(IBaseElement), tuple.Length);

        for (var i = 0; i < tuple.Length; i++)
        {
            tempArr.SetValue(BuildElement(tuple[i], providers), i);
        }

        var elemsList = ((IBaseElement[])tempArr).ToList();

        return new ListElement(elemsList);
    }

    private static DictionaryElement BuildDictionaryElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();

        if (sourceType.IsDictionaryType())
            return BuildDictionaryTypeElement(source, providers);
        else if (sourceType.IsSerializableObject())
            return BuildObjectTypeElement(source, providers);
        else
            throw new NotImplementedException();
    }

    private static DictionaryElement BuildDictionaryTypeElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();

        var count = (int)sourceType.GetProperty("Count").GetValue(source, null);

        var keyType = sourceType.GetGenericArguments()[0];
        if (IBaseElement.PreferredElementType(keyType, providers) != typeof(BasicElement))
            throw new TypeNotSupportException($"Do not support the type of key as {keyType}. Consider a basic type.");

        var tempArr = Array.CreateInstance(typeof(KeyValuePair<BasicElement, IBaseElement>), count);

        var enumerator = sourceType.GetMethod("GetEnumerator").Invoke(source, null);
        for (var i = 0; i < count; i++)
        {
            _ = enumerator.GetType().GetMethod("MoveNext").Invoke(enumerator, null);

            var keyValuePair = enumerator.GetType().GetProperty("Current").GetValue(enumerator, null);
            var key = keyValuePair.GetType().GetProperty("Key").GetValue(keyValuePair, null);
            var value = keyValuePair.GetType().GetProperty("Value").GetValue(keyValuePair, null);

            var elemKeyValuePair = new KeyValuePair<BasicElement, IBaseElement>(
                (BasicElement)BuildElement(key, providers),
                BuildElement(value, providers));
            tempArr.SetValue(elemKeyValuePair, i);
        }

        var elemsDic = new OrderedDictionary<BasicElement, IBaseElement>((KeyValuePair<BasicElement, IBaseElement>[])tempArr);

        return new DictionaryElement(elemsDic);
    }

    private static DictionaryElement BuildObjectTypeElement(object source, IConversionProvider[] providers)
    {
        var sourceType = source.GetType();

        var tempDic = new OrderedDictionary<BasicElement, IBaseElement>();

        foreach (var field in sourceType.GetFields())
        {
            if (field.HasIgnoreAttribute()) continue;

            var key = field.Name;
            var value = field.GetValue(source);

            tempDic.Add(new BasicElement(key), BuildElement(value, providers));
        }

        foreach (var prop in sourceType.GetProperties())
        {
            if (prop.HasIgnoreAttribute() || prop.GetIndexParameters().Length != 0) continue;

            var key = prop.Name;
            var value = prop.GetValue(source);

            tempDic.Add(new BasicElement(key), BuildElement(value, providers));
        }

        return new DictionaryElement(tempDic);
    }
}

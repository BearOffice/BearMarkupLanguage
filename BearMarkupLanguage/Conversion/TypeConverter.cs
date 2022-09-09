using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace BearMarkupLanguage.Conversion;

internal static class TypeConverter
{
    internal static string ConvertToLiteral(object source, IConversionProvider[] providers = null)
    {
        var sourcetype = source.GetType();

        if (providers is not null)
        {
            foreach (var provider in providers)
            {
                if (provider.Type == typeof(object))
                    return provider.ConvertToLiteral(source);

                if (provider.Type.IsGenericTypeDefinition && sourcetype.IsGenericType)
                {
                    if (provider.Type == sourcetype.GetGenericTypeDefinition())
                        return provider.ConvertToLiteral(source);
                }

                if (provider.Type == sourcetype)
                    return provider.ConvertToLiteral(source);
            }
        }

        if (TryConvert(source, typeof(string), out var target))
            return (string)target;
        else
            throw new TypeNotSupportException($"Cannot convert {sourcetype}. " +
                $"Consider add IConvertProvider to support such a type.");
    }

    internal static object ConvertFromLiteral(string source, Type targetType, IConversionProvider[] providers = null)
    {
        if (providers is not null)
        {
            foreach (var provider in providers)
            {
                if (provider.Type == typeof(object))
                    return provider.ConvertFromLiteral(source);

                if (provider.Type.IsGenericTypeDefinition && targetType.IsGenericType)
                {
                    if (provider.Type == targetType.GetGenericTypeDefinition())
                        return provider.ConvertFromLiteral(source);
                }

                if (provider.Type == targetType)
                    return provider.ConvertFromLiteral(source);
            }
        }

        if (TryConvert(source, targetType, out var target))
            return target;
        else
            throw new TypeNotSupportException($"Cannot convert {targetType}. " +
                $"Consider add IConvertProvider to support such a type.");
    }
    
    private static bool TryConvert(object source, Type targetType, out object target)
    {
        var sourceType = source.GetType();

        if (sourceType == targetType)
        {
            target = source;
            return true;
        }

        var targetconverter = TypeDescriptor.GetConverter(targetType);
        if (targetconverter != null && targetconverter.CanConvertFrom(sourceType))
        {
            target = targetconverter.ConvertFrom(source);
            return true;
        }

        var sourceconverter = TypeDescriptor.GetConverter(sourceType);
        if (sourceconverter != null && sourceconverter.CanConvertTo(targetType))
        {
            target = sourceconverter.ConvertTo(source, targetType);

            return true;
        }

        target = default;
        return false;
    }
}

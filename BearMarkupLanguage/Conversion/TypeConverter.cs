using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BearMarkupLanguage.Conversion;

internal static class TypeConverter
{
    internal static string ConvertToLiteral(object source, IConversionProvider[] providers = null)
    {
        var sourceType = source.GetType();

        if (TryGetProvider(sourceType, providers, out var provider))
            return ConvertToLiteralByProvider(source, provider);

        if (TryConvert(source, typeof(string), out var target))
            return (string)target;
        else
            throw new TypeNotSupportException($"Cannot convert {sourceType}. " +
                $"Consider add IConvertProvider to support such a type.");
    }

    internal static object ConvertFromLiteral(string source, Type targetType, IConversionProvider[] providers = null)
    {
        if (TryGetProvider(targetType, providers, out var provider))
            return ConvertFromLiteralByProvider(source, provider);

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

        var targetConverter = TypeDescriptor.GetConverter(targetType);
        if (targetConverter != null && targetConverter.CanConvertFrom(sourceType))
        {
            target = targetConverter.ConvertFrom(source);
            return true;
        }

        var sourceConverter = TypeDescriptor.GetConverter(sourceType);
        if (sourceConverter != null && sourceConverter.CanConvertTo(targetType))
        {
            target = sourceConverter.ConvertTo(source, targetType);

            return true;
        }

        target = default;
        return false;
    }

    internal static bool TryGetProvider(Type type, IConversionProvider[] providers, out IConversionProvider provider)
    {
        if (providers is not null)
        {
            foreach (var p in providers)
            {
                if (p.Type == typeof(object) || p.Type == type ||
                    (type.IsGenericType && p.Type == type.GetGenericTypeDefinition()))
                {
                    provider = p;
                    return true;
                }
            }
        }

        provider = null;
        return false;
    }

    internal static string ConvertToLiteralByProvider(object source, IConversionProvider provider)
    {
        return provider.ConvertToLiteral(source);
    }

    internal static object ConvertFromLiteralByProvider(string source, IConversionProvider provider)
    {
        return provider.ConvertFromLiteral(source);
    }
}

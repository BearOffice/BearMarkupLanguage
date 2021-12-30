using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace BearMLLib.Serialization.Conversion
{
    internal static class TypeConverter
    {
        internal static string ConvertToString(object source)
        {
            return ConvertToString(source, Array.Empty<IConversionProvider>());
        }

        internal static object ConvertFromString(string source, Type targetType)
        {
            return ConvertFromString(source, targetType, Array.Empty<IConversionProvider>());
        }

        internal static string ConvertToString(object source, IConversionProvider provider)
        {
            return ConvertToString(source, new[] { provider });
        }

        internal static object ConvertFromString(string source, Type targetType, IConversionProvider provider)
        {
            return ConvertFromString(source, targetType, new[] { provider });
        }

        internal static string ConvertToString(object source, IConversionProvider[] providers)
        {
            var sourcetype = source.GetType();

            foreach (var provider in providers)
            {
                if (provider.Type == typeof(object))
                    return provider.ConvertToString(source);

                if (provider.Type.IsGenericTypeDefinition && sourcetype.IsGenericType)
                {
                    if (provider.Type == sourcetype.GetGenericTypeDefinition())
                        return provider.ConvertToString(source);
                }

                if (provider.Type == sourcetype)
                    return provider.ConvertToString(source);
            }

            if (TryConvert(source, typeof(string), out var target))
                return (string)target;
            else
                throw new TypeNotSupportException($"Can not convert {sourcetype}. " +
                    $"Consider add IConvertProvider to support such a type.");
        }

        internal static object ConvertFromString(string source, Type targetType, IConversionProvider[] providers)
        {
            foreach (var provider in providers)
            {
                if (provider.Type == typeof(object))
                    return provider.ConvertFromString(source);

                if (provider.Type.IsGenericTypeDefinition && targetType.IsGenericType)
                {
                    if (provider.Type == targetType.GetGenericTypeDefinition())
                        return provider.ConvertFromString(source);
                }

                if (provider.Type == targetType)
                    return provider.ConvertFromString(source);
            }

            if (TryConvert(source, targetType, out var target))
                return target;
            else
                throw new TypeNotSupportException($"Can not convert {targetType}. " +
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
}

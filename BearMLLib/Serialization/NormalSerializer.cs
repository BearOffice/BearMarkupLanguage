using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Serialization.Structure;

namespace BearMLLib.Serialization
{
    internal static class NormalSerializer
    {
        public static object Deserialize(IBasicValue basicValue, Type targetType, IConversionProvider[] providers)
        {
            return TypeConverter.ConvertFromString(basicValue.PlainValue, targetType, providers);
        }

        public static T Deserialize<T>(IBasicValue basicValue, IConversionProvider[] providers)
        {
            return (T)Deserialize(basicValue, typeof(T), providers);
        }

        public static IBasicValue Serialize(object source, Type sourceType, IConversionProvider[] providers)
        {
            var plainValue = TypeConverter.ConvertToString(source, providers);

            return new BasicValue(plainValue);
        }

        public static IBasicValue Serialize<T>(T source, IConversionProvider[] providers)
        {
            return Serialize(source, typeof(T), providers);
        }
    }
}

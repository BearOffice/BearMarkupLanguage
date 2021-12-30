using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Serialization.Conversion;

namespace BearMLLib.Helpers
{
    public static class ListVisualizer
    {
        public static string Visualize(object obj)
        {
            return TypeConverter.ConvertToString(obj, new ListConversionProvider());
        }

        public static string Visualize(object obj, IConversionProvider provider)
        {
            return TypeConverter.ConvertToString(obj, new[] { new ListConversionProvider() , provider});
        }

        public static string Visualize(object obj, IConversionProvider[] providers)
        {
            var listProviders = new[] { new ListConversionProvider() };
            return TypeConverter.ConvertToString(obj, listProviders.Concat(providers).ToArray());
        }
    }
}

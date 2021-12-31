using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Serialization.Conversion;

namespace BearMLLib.Helpers
{
    /// <summary>
    /// Visualize the list.
    /// </summary>
    public static class ListVisualizer
    {
        /// <summary>
        /// Visualize the list.
        /// </summary>
        /// <param name="obj">The object types <see cref="List{T}"/></param>
        /// <returns></returns>
        public static string Visualize(object obj)
        {
            return TypeConverter.ConvertToString(obj, new ListConversionProvider());
        }
        /// <summary>
        /// Visualize the list.
        /// </summary>
        /// <param name="obj">The object types <see cref="List{T}"/></param>
        /// <param name="provider"><see cref="ConversionProvider"/></param>
        /// <returns></returns>
        public static string Visualize(object obj, IConversionProvider provider)
        {
            return TypeConverter.ConvertToString(obj, new[] { new ListConversionProvider() , provider});
        }

        /// <summary>
        /// Visualize the list.
        /// </summary>
        /// <param name="obj">The object types <see cref="List{T}"/></param>
        /// <param name="providers"><see cref="Array"/> of <see cref="ConversionProvider"/></param>
        /// <returns></returns>
        public static string Visualize(object obj, IConversionProvider[] providers)
        {
            var listProviders = new[] { new ListConversionProvider() };
            return TypeConverter.ConvertToString(obj, listProviders.Concat(providers).ToArray());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Conversion
{
    /// <summary>
    /// Provide the method to 
    /// convert specified type to <see cref="string"/> and from <see cref="string"/>.
    /// </summary>
    public interface IConversionProvider
    {
        /// <summary>
        /// <see cref="System.Type"/> of the conversion target.
        /// </summary>
        public Type Type { get; }
        /// <summary>
        /// Convert source object to string.
        /// </summary>
        /// <param name="source">Source object to be convert.</param>
        /// <returns>The converted result.</returns>
        public string ConvertToString(object source);
        /// <summary>
        /// Convert source object from string.
        /// </summary>
        /// <param name="source">string to be convert.</param>
        /// <returns>The converted result.</returns>
        public object ConvertFromString(string source);
    }
}

using System;
using System.Collections.Generic;
using System.Text;


namespace BearMLLib.Serialization.Conversion
{
    /// <summary>
    /// Provide the method to 
    /// convert specified type to <see cref="string"/> and from <see cref="string"/>.
    /// </summary>
    public class ConversionProvider : IConversionProvider
    {
        private readonly Func<string, object> _fromStringConverter;
        private readonly Func<object, string> _toStringConverter;
        /// <summary>
        /// <see cref="System.Type"/> of the conversion target.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Provide the method to 
        /// convert specified type to <see cref="string"/> and from <see cref="string"/>.
        /// </summary>
        /// <param name="type">source type</param>
        /// <param name="fromStringConverter">Func that convert source type to string.</param>
        /// <param name="toStringConverter">Func that convert source type from string.</param>
        public ConversionProvider(Type type, Func<string, object> fromStringConverter, 
            Func<object, string> toStringConverter)
        {
            _fromStringConverter = fromStringConverter;
            _toStringConverter = toStringConverter;
            Type = type;
        }

        /// <summary>
        /// Convert source object to string.
        /// </summary>
        /// <param name="source">Source object to be convert.</param>
        /// <returns>The converted result.</returns>
        public string ConvertToString(object source)
        {
            return _toStringConverter.Invoke(source);
        }

        /// <summary>
        /// Convert source object from string.
        /// </summary>
        /// <param name="source">string to be convert.</param>
        /// <returns>The converted result.</returns>
        public object ConvertFromString(string source)
        {
            return _fromStringConverter.Invoke(source);
        }
    }
}

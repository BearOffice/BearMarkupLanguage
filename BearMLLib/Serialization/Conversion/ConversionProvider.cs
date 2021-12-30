using System;
using System.Collections.Generic;
using System.Text;


namespace BearMLLib.Serialization.Conversion
{
    public class ConversionProvider : IConversionProvider
    {
        private Func<string, object> _fromStringConverter;
        private Func<object, string> _toStringConverter;
        public Type Type { get; }

        public ConversionProvider(Type type, Func<string, object> fromstringconverter, 
            Func<object, string> tostringconverter)
        {
            _fromStringConverter = fromstringconverter;
            _toStringConverter = tostringconverter;
            Type = type;
        }

        public string ConvertToString(object source)
        {
            return _toStringConverter.Invoke(source);
        }

        public object ConvertFromString(string source)
        {
            return _fromStringConverter.Invoke(source);
        }
    }
}

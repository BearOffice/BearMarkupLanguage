using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Conversion
{
    public interface IConversionProvider
    {
        public Type Type { get; }
        public string ConvertToString(object source);
        public object ConvertFromString(string source);
    }
}

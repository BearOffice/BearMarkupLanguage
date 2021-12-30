using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Serialization.Conversion;

namespace BearMLLib.Configs
{
    internal interface IContent
    {
        ContentType Type { get; }
        T Get<T>(IConversionProvider[] providers);
        object Get(Type targetType, IConversionProvider[] providers);
    }
}

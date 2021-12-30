using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Core.Helpers;
using BearMLLib.Serialization;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Serialization.Structure;

namespace BearMLLib.Configs
{
    internal class LiterialValue : IContent
    {
        public ContentType Type => ContentType.LiterialValue;
        internal IBasicValue BaseValue { get; }
        
        internal LiterialValue(IBasicValue baseValue)
        {
            if (!IsValidBasicValue(baseValue))
                throw new InvalidConfigException("The base value is invalid.");

            BaseValue = baseValue;
        }

        public T Get<T>(IConversionProvider[] providers)
        {
            return NormalSerializer.Deserialize<T>(BaseValue, providers);
        }

        public object Get(Type targetType, IConversionProvider[] providers)
        {
            return NormalSerializer.Deserialize(BaseValue, targetType, providers);
        }

        internal static bool IsValidBasicValue(IBasicValue basicValue)
        {
            return basicValue.PlainValue != null;
        }
    }
}

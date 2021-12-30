using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Structure
{
    internal struct BasicValue : IBasicValue
    {
        public StructureType Type => StructureType.Normal;
        public string PlainValue { get; }

        internal BasicValue(string plainValue)
        {
            PlainValue = plainValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Structure
{
    internal interface IBasicValue : IStructure
    {
        string PlainValue { get; }
    }
}

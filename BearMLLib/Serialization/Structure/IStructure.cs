using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Structure
{
    internal interface IStructure
    {
        StructureType Type { get; }
    }
}

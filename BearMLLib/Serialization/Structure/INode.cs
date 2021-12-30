using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Structure
{
    internal interface INode : IStructure
    {
        List<INode> Nodes { get; }
        IBasicValue BasicValue { get; }
    }
}

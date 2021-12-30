using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization.Structure
{
    internal struct Node : INode
    {
        public StructureType Type => StructureType.Recursive;
        public List<INode> Nodes { get; }
        public IBasicValue BasicValue { get; }

        internal Node(List<INode> nodes)
        {
            Nodes = nodes;
            BasicValue = null;
        }
        
        internal Node(IBasicValue value)
        {
            Nodes = null;
            BasicValue = value;
        }
    }
}

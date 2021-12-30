using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Core.Helpers;
using BearMLLib.Serialization;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Serialization.Structure;

namespace BearMLLib.Configs
{
    internal class ExpandedList : IContent
    {
        public ContentType Type => ContentType.ExpandedList;
        internal INode Node { get; }

        internal ExpandedList(INode node)
        {
            if (!IsValidNode(node))
                throw new InvalidConfigException("The node is invalid.");

            Node = node;
        }

        public T Get<T>(IConversionProvider[] providers)
        {
            return RecursiveSerializer.Deserialize<T>(Node, providers);
        }

        public object Get(Type targetType, IConversionProvider[] providers)
        {
            return RecursiveSerializer.Deserialize(Node, targetType, providers);
        }

        internal static bool IsValidNode(INode node)
        {
            return RecursiveSerializer.IsValidNode(node, out _);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BearMLLib.Core.Helpers;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Serialization.Structure;


namespace BearMLLib.Serialization
{
    internal static class RecursiveSerializer
    {
        public static object Deserialize(INode node, Type targetType, IConversionProvider[] providers)
        {
            if (!IsValidNode(node, out var depth))
                throw new ArgumentException("Node is not valid.");

            if (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(List<>))
                throw new TypeNotSupportException($"Cannot deserialize to {targetType}. {typeof(List<>)} is required.");

            if (depth != GetListDepth(targetType))
                throw new TypeNotSupportException("The given type is inconsistent with the configs.");

            return ListDeserialize(node, targetType, providers);
        }

        public static T Deserialize<T>(INode node, IConversionProvider[] providers)
        {
            return (T)Deserialize(node, typeof(T), providers);
        }

        public static INode Serialize(object source, Type sourceType, IConversionProvider[] providers)
        {
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(List<>))
                throw new TypeNotSupportException($"Cannot serialize from {sourceType}. {typeof(List<>)} is required.");

            return ListSerialize(source, providers);
        }

        public static INode Serialize<T>(T source, IConversionProvider[] providers)
        {
            return Serialize(source, typeof(T), providers);
        }

        private static Node ListSerialize<T>(T source, IConversionProvider[] providers)
        {
            var sourceType = source.GetType();

            var count = (int)sourceType.GetProperty("Count").GetValue(source, new object[] { });

            var listNode = new Node(new List<INode>());

            var argument = sourceType.GetGenericArguments()[0];
            if (argument.IsGenericType && argument.GetGenericTypeDefinition() == typeof(List<>))
            {
                    for (var i = 0; i < count; i++)
                    {
                        var item = sourceType.GetProperty("Item").GetValue(source, new object[] { i });
                        var node = ListSerialize(item, providers);
                        listNode.Nodes.Add(node);
                    }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var item = sourceType.GetProperty("Item").GetValue(source, new object[] { i });
                    var value = NormalSerializer.Serialize(item, providers);
                    listNode.Nodes.Add(new Node(value));
                }
            }

            return listNode;
        }

        private static object ListDeserialize(INode node, Type targetType, IConversionProvider[] providers)
        {
            var rootList = Activator.CreateInstance(targetType);

            var argument = targetType.GetGenericArguments()[0];
            if (argument.IsGenericType && argument.GetGenericTypeDefinition() == typeof(List<>))
            {
                foreach (var n in node.Nodes)
                {
                    var list = ListDeserialize(n, argument, providers);
                    targetType.GetMethod("Add").Invoke(rootList, new object[] { list });
                }
            }
            else
            {
                foreach (var n in node.Nodes)
                {
                    var value = TypeConverter.ConvertFromString(n.BasicValue.PlainValue, argument, providers);
                    targetType.GetMethod("Add").Invoke(rootList, new object[] { value });
                }
            }

            return rootList;
        }

        internal static bool IsValidNode(INode node, out int depth)
        {
            if (node.Nodes != null)
            {
                // allow empty list
                if (node.Nodes.Count == 0)
                {
                    depth = 1;
                    return true;
                }

                if (!IsValidNode(node.Nodes[0], out var prevDepth))
                {
                    depth = -1;
                    return false;
                }

                foreach (var n in node.Nodes.Skip(1))
                {
                    if (!IsValidNode(n, out var nDepth) || nDepth != prevDepth)
                    {
                        depth = -1;
                        return false;
                    }
                }
                depth = prevDepth + 1;
                return true;
            }
            else
            {
                depth = 0;
                return true;
            }
        }

        internal static int GetLengthOfNode(INode node)
        {
            if (node.Nodes != null)
                return node.Nodes.Count;
            else
                return 0;
        }

        internal static int GetMaximumLengthOfNode(INode node)
        {
            var length = 0;

            if (node.Nodes != null)
            {
                length = node.Nodes.Count;

                foreach (var n in node.Nodes)
                {
                    var maxLen = GetMaximumLengthOfNode(n);
                    length = length < maxLen ? maxLen : length;
                }

                return length;
            }
            else
            {
                return length;
            }
        }

        // List<> -> depth = 1
        private static int GetListDepth(Type type)
        {
            var depth = 0;

            var argument = type.GetGenericArguments()[0];

            while (true)
            {
                if (argument.IsGenericType && argument.GetGenericTypeDefinition() == typeof(List<>))
                {
                    argument = argument.GetGenericArguments()[0];
                    depth++;
                }
                else
                {
                    depth++;
                    return depth;
                }
            }
        }

        private static Type GenerateListType(int depth)
        {
            if (depth <= 0) throw new ArgumentOutOfRangeException();

            var listType = typeof(string);

            while (depth > 0)
            {
                listType = typeof(List<>).MakeGenericType(new[] { listType });
                depth--;
            }

            return listType;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using BearMLLib.Configs;
using BearMLLib.Serialization;
using BearMLLib.Serialization.Structure;
using BearMLLib.Serialization.Conversion;
using BearMLLib.Text;

namespace BearMLLib.Core
{
    internal static class ContentTypeSelector
    {
        internal static ContentType GetContentType(IStructure structure)
        {
            switch (structure.Type)
            {
                case StructureType.Normal:
                    var baseValue = structure as IBasicValue;
                    var plainValue = baseValue.PlainValue;

                    switch (Format.This.ValueMode)
                    {
                        case ValueMode.Auto:
                            if (plainValue.Escape(EscapeLevel.HC) == plainValue.Unescape() &&
                                !plainValue.IsNullOrWhiteSpace())
                                return ContentType.EscapedValue;
                            else
                                return ContentType.LiterialValue;

                        case ValueMode.ForceEscaped:
                            return ContentType.EscapedValue;

                        case ValueMode.ForceLiterial:
                            return ContentType.LiterialValue;

                        default:
                            throw new NotSupportedException();
                    }

                case StructureType.Recursive:
                    var node = structure as INode;
                    var lengthOfNode = RecursiveSerializer.GetLengthOfNode(node);

                    switch (Format.This.ListMode)
                    {
                        case ListMode.Auto:
                            if (lengthOfNode > Format.This.MaximumElementNumber)
                                return ContentType.FoldedList;
                            else
                                return ContentType.ExpandedList;

                        case ListMode.ForceFolded:
                            return ContentType.FoldedList;

                        case ListMode.ForceExpanded:
                            return ContentType.ExpandedList;

                        default:
                            throw new NotSupportedException();
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        internal static ContentType GetContentType(IContent content)
        {
            switch (content.Type)
            {
                case ContentType.EscapedValue:
                    var escapedValue = content as EscapedValue;
                    return GetContentType(escapedValue.BaseValue);

                case ContentType.LiterialValue:
                    var literialValue = content as LiterialValue;
                    return GetContentType(literialValue.BaseValue);

                case ContentType.ExpandedList:
                    var expandedList = content as ExpandedList;
                    return GetContentType(expandedList.Node);

                case ContentType.FoldedList:
                    var foldedList = content as FoldedList;
                    return GetContentType(foldedList.Node);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static IContent GetContent(IStructure structure)
        {
            return GetContentType(structure) switch
            {
                ContentType.EscapedValue => new EscapedValue(structure as IBasicValue),
                ContentType.LiterialValue => new LiterialValue(structure as IBasicValue),
                ContentType.ExpandedList => new ExpandedList(structure as INode),
                ContentType.FoldedList => new FoldedList(structure as INode),
                _ => throw new NotSupportedException(),
            };
        }

        internal static IContent GetContent(IContent content)
        {
            switch (content.Type)
            {
                case ContentType.EscapedValue:
                    var escapedValue = content as EscapedValue;
                    return GetContent(escapedValue.BaseValue);

                case ContentType.LiterialValue:
                    var literialValue = content as LiterialValue;
                    return GetContent(literialValue.BaseValue);

                case ContentType.ExpandedList:
                    var expandedList = content as ExpandedList;
                    return GetContent(expandedList.Node);

                case ContentType.FoldedList:
                    var foldedValue = content as FoldedList;
                    return GetContent(foldedValue.Node);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static IContent GetContent(object source, Type sourceType, IConversionProvider[] providers)
        {
            var structureType = SelectStructureType(sourceType);

            switch (structureType)
            {
                case StructureType.Normal:
                    var baseValue = NormalSerializer.Serialize(source, sourceType, providers);
                    return GetContent(baseValue);

                case StructureType.Recursive:
                    var node = RecursiveSerializer.Serialize(source, sourceType, providers);
                    return GetContent(node);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static IContent GetContent<T>(T source, IConversionProvider[] providers)
        {
            return GetContent(source, typeof(T), providers);
        }

        internal static StructureType SelectStructureType(Type sourceType)
        {
            if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(List<>))
                return StructureType.Recursive;
            else
                return StructureType.Normal;
        }
    }
}

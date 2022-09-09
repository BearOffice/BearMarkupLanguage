using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Core;
using BearMarkupLanguage.Text;
using BearMarkupLanguage.Helpers;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BearMarkupLanguage.Elements;

internal class ListElement: IBaseElement
{
    internal List<IBaseElement> ElementsList { get; private init; }
    public ParseMode PreferredParseMode
    {
        get
        {
            if (ElementsList.Count != 0)
            {
                switch (Format.PrintMode)
                {
                    case PrintMode.Auto:
                        if (ElementsList.Count <= Format.MaxElementsNumber)
                            return ParseMode.Expand;
                        else
                            return ParseMode.Collapse;
                    case PrintMode.Compact:
                        return ParseMode.Collapse;
                    case PrintMode.Expand:
                        return ParseMode.Expand;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return ParseMode.Collapse;  // cannot display empty list
            }
        }
    }

    internal ListElement()
    {
        ElementsList = new List<IBaseElement>();
    }

    internal ListElement(List<IBaseElement> elementsList)
    {
        ElementsList = elementsList;
    }

    public object ConvertTo(Type targetType, IConversionProvider[] providers) 
    {
        if (targetType.IsListType())
        {
            if (!targetType.IsSZArray)
                throw new TypeNotSupportException("Do not support multi-dimensional array.");

            return ConvertIListTo(targetType, providers);
        }
        else if (targetType.IsTupleType())
        {
            return ConvertITupleTo(targetType, providers);
        }
        else
        {
            throw new TypeNotMatchException("Type IList or ITuple was expected, " +
                $"but type {targetType} was specified.");
        }
    }

    private object ConvertIListTo(Type targetType, IConversionProvider[] providers)
    {
        Type argumentType;
        if (targetType.IsSZArray)
        {
            argumentType = targetType.GetElementType();  // Array needs a special treatment
        }
        else
        {
            argumentType = targetType.GetGenericArguments()[0];  // IList only contains one generic argument
        }

        var preferredType = IBaseElement.PreferredElementType(argumentType);
        var targetArr = Array.CreateInstance(argumentType, ElementsList.Count);

        for (var i = 0; i < ElementsList.Count; i++)
        {
            if (ElementsList[i].GetType() != preferredType && ElementsList[i].GetType() != typeof(EmptyElement))
                throw new TypeNotMatchException($"Type of element not match.");

            targetArr.SetValue(ElementsList[i].ConvertTo(argumentType, providers), i);
        }

        if (targetType.IsArray)  // array and other ILists should be treated in different ways
        {
            return targetArr;
        }
        else
        {
            var consInfo = targetType.GetConstructor(new[] { targetArr.GetType() });

            if (consInfo is null)
                throw new TypeNotMatchException($"No constructor found for Type {targetType}.");

            return consInfo.Invoke(new[] { targetArr });
        }
    }

    private object ConvertITupleTo(Type targetType, IConversionProvider[] providers)
    {
        var argumentsType = targetType.GetGenericArguments();

        if (argumentsType.Length < 8)
        {
            if (ElementsList.Count != argumentsType.Length)
                throw new TypeNotMatchException($"Type of tuple not match.");
        }
        else
        {
            // Compiler will explain (..7 elements.., (..)) as (..7 elements.., ((..)))
            // The following code is to fix the above problem.
            var lastArgs = argumentsType[7].GetGenericArguments();
            if (lastArgs.Length == 1 && lastArgs[0].IsTupleType())
                argumentsType[7] = lastArgs[0];

            if (ElementsList.Count < argumentsType.Length)
                throw new TypeNotMatchException($"Type of tuple not match.");
        }

        var objArr = new object[argumentsType.Length];

        if (argumentsType.Length < 8)
        {
            for (var i = 0; i < argumentsType.Length; i++)
            {
                objArr[i] = ElementsList[i].ConvertTo(argumentsType[i], providers);
            }
        }
        else
        {
            for (var i = 0; i < 7; i++)
            {
                objArr[i] = ElementsList[i].ConvertTo(argumentsType[i], providers);
            }

            if (ElementsList.Count == 8 && ElementsList[^1].GetType() == typeof(ListElement))
            {
                objArr[7] = ElementsList[^1].ConvertTo(argumentsType[7], providers);
            }
            else
            {
                var tempTupleElem = new ListElement(ElementsList.ToArray()[7..].ToList());
                objArr[7] = tempTupleElem.ConvertTo(argumentsType[7], providers);
            }
        }

        var tupleType = targetType.GetStaticTupleType();
        var tuple = tupleType.GetMethods()
                             .First(item => item.Name == "Create"
                                    && item.GetParameters().Length == objArr.Length
                                    && item.IsGenericMethod)
                             .MakeGenericMethod(argumentsType)
                             .Invoke(null, objArr);

        return tuple;
    }

    public string[] ParseToLiteral(ParseMode mode)
    {
        return mode switch
        {
            ParseMode.Collapse => CollapsedParse(),
            ParseMode.Expand => ExpandedParse(),
            _ => throw new NotImplementedException()
        };
    }

    private string[] CollapsedParse()
    {
        var sb = new StringBuilder();
        sb.Append(ID.CollapsedListNodeL);

        foreach (var elem in ElementsList.SkipLast(1))
        {
            sb.Append(Parse(elem));
            sb.Append(ID.CollapsedElementSplitSymbol).Append(' ');
        }

        if (ElementsList.Count > 0)
        {
            sb.Append(Parse(ElementsList[^1]));
        }

        sb.Append(ID.CollapsedListNodeR);

        return new[] { sb.ToString() };


        static StringBuilder Parse(IBaseElement element)
        {
            var sb = new StringBuilder();
            var elemLiteral = element.ParseToLiteral(ParseMode.Collapse)[0];

            if (element is BasicElement)
            {
                sb.Append(ID.CollapsedBasicElementNode);
                sb.Append(elemLiteral.EscapeChar(ID.CollapsedBasicElementNode));
                sb.Append(ID.CollapsedBasicElementNode);
            }
            else
            {
                sb.Append(elemLiteral);
            }

            return sb;
        }
    }

    private string[] ExpandedParse()
    {
        var tempList = new List<string>();

        foreach (var elem in ElementsList)
        {
            var elemParseMode = elem.PreferredParseMode;
            var elemLiteral = elem.ParseToLiteral(elemParseMode);

            if (elem is BasicElement)
            {
                switch (elemParseMode)
                {
                    case ParseMode.Collapse:
                        tempList.Add(ID.ExpandedListNode + " " + elemLiteral[0]);
                        break;
                    case ParseMode.Expand:
                        tempList.Add(ID.ExpandedListNode + " " + ID.LiteralElementSymbol);
                        tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
                        tempList.Add(ID.EndOfLine.ToString());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (elem is EmptyElement)
            {
                tempList.Add(ID.ExpandedListNode + " " + elemLiteral[0]);
            }
            else if (elem is DictionaryElement)
            {
                tempList.Add(ID.ExpandedListNode + " " + ID.ExpandedDicSymbol);
                tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
            }
            else
            {
                tempList.Add(ID.ExpandedListNode.ToString());
                tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
            }
        }

        return tempList.ToArray();
    }
}

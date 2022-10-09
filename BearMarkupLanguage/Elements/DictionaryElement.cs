using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearMarkupLanguage.Core;
using BearMarkupLanguage.Text;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Conversion;

namespace BearMarkupLanguage.Elements;

internal class DictionaryElement : IBaseElement
{
    internal OrderedDictionary<BasicElement, IBaseElement> ElementsDic { get; private init; }
    public ParseMode PreferredParseMode
    {
        get
        {
            if (ElementsDic.Count != 0 && !IsKeysNeedCollapse())
            {
                switch (Format.PrintMode)
                {
                    case PrintMode.Auto:
                        if (ElementsDic.Count > Format.MaxElementsNumber)
                            return ParseMode.Collapse;
                        else
                            return ParseMode.Expand;
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
                return ParseMode.Collapse;  // cannot display empty dic nor white space key
            }
        }
    }

    internal DictionaryElement()
    {
        ElementsDic = new OrderedDictionary<BasicElement, IBaseElement>();
    }

    internal DictionaryElement(OrderedDictionary<BasicElement, IBaseElement> elementsDic)
    {
        ElementsDic = elementsDic;
    }

    public object ConvertTo(Type targetType, IConversionProvider[] providers)
    {
        if (targetType.IsDictionaryType())
        {
            return ConvertIDictionaryTo(targetType, providers);
        }
        else if (targetType.IsSerializableObject())
        {
            return ConvertObjectTo(targetType, providers);
        }
        else
        {
            throw new TypeNotMatchException("Type IDictionary or serializable object was expected, " +
                $"but type {targetType} was specified.");
        }
    }

    private object ConvertIDictionaryTo(Type targetType, IConversionProvider[] providers)
    {
        // IDictionary contains two generic arguments
        var arguments = targetType.GetGenericArguments();
        // try to find key and value types in target type if target type is not generic / doesn't contain 2 arguments
        if (arguments.Length < 2) 
        {
            var iDicInterface = targetType.GetInterfaces()
                                          .FirstOrDefault(i => i.GetGenericTypeDefinitionIfHas() == typeof(IDictionary<,>));
            arguments = iDicInterface?.GetGenericArguments();
        }
        if (arguments is null || arguments.Length < 2)
            throw new TypeNotSupportException($"Cannot find key or value type in Type {targetType}");
        
        var keyType = arguments[0];
        var valueType = arguments[1];

        var preferredKeyType = IBaseElement.PreferredElementType(keyType);
        var preferredValueType = IBaseElement.PreferredElementType(valueType);

        if (preferredKeyType != typeof(BasicElement))
            throw new TypeNotSupportException($"Do not support the type of key as {keyType}. Consider a basic type.");

        var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(new[] { keyType, valueType });
        var targetArr = Array.CreateInstance(keyValueType, ElementsDic.Count);

        var keys = ElementsDic.Keys.ToList();
        var values = ElementsDic.Values.ToList();
        for (var i = 0; i < ElementsDic.Count; i++)
        {
            if (keys[i].GetType() != preferredKeyType)
                throw new TypeNotMatchException($"Type of key not match.");
            if (values[i].GetType() != preferredValueType && values[i].GetType() != typeof(EmptyElement))
                throw new TypeNotMatchException($"Type of value not match.");

            var keyValuePairconsInfo = keyValueType.GetConstructor(new[] { keyType, valueType });
            var keyValuePair = keyValuePairconsInfo.Invoke(new[] {
                keys[i].ConvertTo(keyType, providers),
                values[i].ConvertTo(valueType, providers)
            });

            targetArr.SetValue(keyValuePair, i);
        }

        var consInfo = targetType.GetConstructor(new[] { targetArr.GetType() });

        if (consInfo is null)
            throw new TypeNotMatchException($"No constructor found for Type {targetType}.");

        return consInfo.Invoke(new[] { targetArr });
    }

    private object ConvertObjectTo(Type targetType, IConversionProvider[] providers)
    {
        var target = Activator.CreateInstance(targetType);
        var count = 0;
        foreach (var field in targetType.GetFields())
        {
            if (field.HasIgnoreAttribute()) continue;

            var name = field.Name;
            var type = field.FieldType;

            var key = new BasicElement(name);
            var result = ElementsDic.TryGetValue(key, out var value);

            if (!result) throw new TypeNotMatchException($"Field {name} not found.");

            field.SetValue(target, value.ConvertTo(type, providers));
            count++;
        }

        foreach (var prop in targetType.GetProperties())
        {
            if (prop.HasIgnoreAttribute() || prop.GetIndexParameters().Length != 0) continue;

            var name = prop.Name;
            var type = prop.PropertyType;

            var key = new BasicElement(name);
            var result = ElementsDic.TryGetValue(key, out var value);

            if (!result) throw new TypeNotMatchException($"Property {name} not found.");

            prop.SetValue(target, value.ConvertTo(type, providers));
            count++;
        }

        if (ElementsDic.Count != count)
            throw new TypeNotMatchException("Fields and properties' number is inconsistent with the given type.");

        return target;
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
        sb.Append(ID.CollapsedDicNodeL);

        foreach ((var keyElem, var elem) in ElementsDic.SkipLast(1))
        {
            sb.Append(Parse(keyElem, elem));
            sb.Append(ID.CollapsedElementSplitSymbol).Append(' ');
        }

        if (ElementsDic.Count > 0)
        {
            (var keyElem, var elem) = ElementsDic.TakeLast(1).ToArray()[0];
            sb.Append(Parse(keyElem, elem));
        }

        sb.Append(ID.CollapsedDicNodeR);

        return new[] { sb.ToString() };


        static StringBuilder Parse(BasicElement keyElement, IBaseElement element)
        {
            var sb = new StringBuilder();

            var key = keyElement.ParseToLiteral(ParseMode.Collapse)[0];
            var elemLiteral = element.ParseToLiteral(ParseMode.Collapse)[0];

            sb.Append(ID.CollapsedBasicElementNode);
            sb.Append(key.EscapeChar(ID.CollapsedBasicElementNode));
            sb.Append(ID.CollapsedBasicElementNode);
            sb.Append(ID.Key).Append(' ');

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

        foreach ((var keyElem, var elem) in ElementsDic)
        {
            var key = keyElem.ParseToLiteral(ParseMode.Collapse)[0].EscapeChar(ID.Key);

            var elemParseMode = elem.PreferredParseMode;
            var elemLiteral = elem.ParseToLiteral(elemParseMode);

            if (elem is BasicElement)
            {
                switch (elemParseMode)
                {
                    case ParseMode.Collapse:
                        tempList.Add(key + ID.Key + " " + elemLiteral[0]);
                        break;
                    case ParseMode.Expand:
                        tempList.Add(key + ID.Key + " " + ID.LiteralElementSymbol);
                        tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
                        tempList.Add(ID.EndOfLine.ToString());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (elem is EmptyElement)
            {
                tempList.Add(key + ID.Key + " " + elemLiteral[0]);
            }
            else if (elem is DictionaryElement)
            {
                tempList.Add(key + ID.Key + " " + ID.ExpandedDicSymbol);
                tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
            }
            else
            {
                tempList.Add(key + ID.Key);
                tempList.AddRange(elemLiteral.IncrOrDecrDepth(1));
            }
        }

        return tempList.ToArray();
    }

    private bool IsKeysNeedCollapse()
    {
        foreach ((var key, _) in ElementsDic)
        {
            if (key.Literal.IsNullOrWhiteSpace()
                || key.Literal.StartsWith(' ') || key.Literal.EndsWith(' '))
                return true;
        }

        return false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Core;
using BearMarkupLanguage.Text;

namespace BearMarkupLanguage.Elements;

internal class BasicElement : IBaseElement, IEquatable<BasicElement>
{
    internal string Literal { get; private init; }
    public ParseMode PreferredParseMode
    {
        get
        {
            if (Literal.IsNullOrWhiteSpace() || Literal.StartsWith(' ') || Literal.EndsWith(' '))
            {
                return ParseMode.Expand;
            }
            else
            {
                return Format.PrintMode switch
                {
                    PrintMode.Auto =>
                        Literal.ContainsEscapeChar(EscapeLevel.CollapsedBasic) ? ParseMode.Expand : ParseMode.Collapse,
                    PrintMode.Compact => ParseMode.Collapse,
                    PrintMode.Expand => ParseMode.Expand,
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }

    internal BasicElement()
    {
        Literal = "";
    }

    internal BasicElement(string literal)
    {
        Literal = literal;
    }

    public object ConvertTo(Type targetType, IConversionProvider[] providers)
    {
        if (IBaseElement.PreferredElementType(targetType) != typeof(BasicElement))
            throw new TypeNotMatchException($"A basic type was expected, but type {targetType} was specified.");

        return TypeConverter.ConvertFromLiteral(Literal, targetType, providers);
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
        return new[] { Literal.Escape(EscapeLevel.CollapsedBasic) };
    }

    private string[] ExpandedParse()
    {
        var literal = Literal.SplitToLines();
        return literal;
    }

    public bool Equals(BasicElement other)
    {
        return Literal == other.Literal;
    }

    public override bool Equals(object obj)
    {
        if (obj is BasicElement elem)
            return Equals(elem);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Literal.GetHashCode();
    }

    public static bool operator ==(BasicElement elemL, BasicElement elemR)
    {
        return elemL.Equals(elemR);
    }

    public static bool operator !=(BasicElement elemL, BasicElement elemR)
    {
        return !elemL.Equals(elemR);
    }
}

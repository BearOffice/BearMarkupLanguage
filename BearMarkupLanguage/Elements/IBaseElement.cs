using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BearMarkupLanguage.Conversion;
using BearMarkupLanguage.Helpers;
using BearMarkupLanguage.Serialization;
using SerializableAttribute = BearMarkupLanguage.Serialization.SerializableAttribute;

namespace BearMarkupLanguage.Elements;

internal interface IBaseElement
{
    public ParseMode PreferredParseMode { get; }
    public object ConvertTo(Type targetType, IConversionProvider[] providers);
    public string[] ParseToLiteral(ParseMode mode);

    internal static Type PreferredElementType(Type type)
    {
        if (type.IsListType())
        {
            if (type.IsVariableBoundArray)  // Do not support multi-dimensional array
                return typeof(BasicElement);

            return typeof(ListElement);
        }
        else if (type.IsTupleType())
        {
            return typeof(ListElement);
        }
        else if (type.IsDictionaryType())
        {
            return typeof(DictionaryElement);
        }
        else if (type.IsSerializableObject())
        {
            return typeof(DictionaryElement);
        }
        else
        {
            return typeof(BasicElement);
        }
    }
}
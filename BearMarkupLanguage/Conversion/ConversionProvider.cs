using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Conversion;

/// <summary>
/// Provides the method to convert specified type to literal and from literal.
/// </summary>
public class ConversionProvider : IConversionProvider
{
    private readonly Func<string, object> _fromLiteralConverter;
    private readonly Func<object, string> _toLiteralConverter;
    /// <summary>
    /// Type of the conversion target.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Provides the method to convert specified type to literal and from literal.
    /// </summary>
    /// <param name="type">Type of the conversion target.</param>
    /// <param name="fromLiteralConverter">Method to convert source object to literal.</param>
    /// <param name="toLiteralConverter">Method to convert source object from literal.</param>
    public ConversionProvider(Type type, Func<string, object> fromLiteralConverter, 
        Func<object, string> toLiteralConverter)
    {
        _fromLiteralConverter = fromLiteralConverter;
        _toLiteralConverter = toLiteralConverter;
        Type = type;
    }

    /// <summary>
    /// Convert source object to literal.
    /// </summary>
    /// <param name="source">Source object to be convert.</param>
    /// <returns>Result of the conversion.</returns>
    public string ConvertToLiteral(object source)
    {
        return _toLiteralConverter.Invoke(source);
    }

    /// <summary>
    /// Convert source object from literal.
    /// </summary>
    /// <param name="literal">Literal to be convert.</param>
    /// <returns>Result of the conversion.</returns>
    public object ConvertFromLiteral(string literal)
    {
        return _fromLiteralConverter.Invoke(literal);
    }
}

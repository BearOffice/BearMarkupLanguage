using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Conversion;

/// <summary>
/// Provides the method to convert specified type to literal and from literal.
/// </summary>
public interface IConversionProvider
{
    /// <summary>
    /// Type of the conversion target.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Convert source object to literal.
    /// </summary>
    /// <param name="source">Source object to be convert.</param>
    /// <returns>Result of the conversion.</returns>
    public string ConvertToLiteral(object source);

    /// <summary>
    /// Convert source object from literal.
    /// </summary>
    /// <param name="literal">Literal to be convert.</param>
    /// <returns>Result of the conversion.</returns>
    public object ConvertFromLiteral(string literal);
}

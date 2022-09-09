using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Conversion;

/// <summary>
/// The exception that is thrown when type is not supported.
/// </summary>
public class TypeNotSupportException : Exception
{
    /// <summary>
    /// The exception that is thrown when type is not supported.
    /// </summary>
    public TypeNotSupportException() : base() { }

    /// <summary>
    /// The exception that is thrown when type is not supported.
    /// </summary>
    public TypeNotSupportException(string message) : base(message) { }

    /// <summary>
    /// The exception that is thrown when type is not supported.
    /// </summary>
    public TypeNotSupportException(string message, Exception innerException)
        : base(message, innerException) { }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Conversion;

/// <summary>
/// The exception that is thrown when type do not match.
/// </summary>
public class TypeNotMatchException : Exception
{
    /// <summary>
    /// The exception that is thrown when type do not match.
    /// </summary>
    public TypeNotMatchException() : base() { }

    /// <summary>
    /// The exception that is thrown when type do not match.
    /// </summary>
    public TypeNotMatchException(string message) : base(message) { }

    /// <summary>
    /// The exception that is thrown when type do not match.
    /// </summary>
    public TypeNotMatchException(string message, Exception innerException)
        : base(message, innerException) { }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BearMarkupLanguage.Interpretation.Helpers;

/// <summary>
/// The exception that is thrown when the literal in bear markup language has an invalid format.
/// </summary>
public class InvalidFormatException : Exception
{
    /// <summary>
    /// The exception that is thrown when the literal in bear markup language has an invalid format.
    /// </summary>
    public InvalidFormatException() : base() { }

    /// <summary>
    /// The exception that is thrown when the literal in bear markup language has an invalid format.
    /// </summary>
    public InvalidFormatException(string message) : base(message) { }

    /// <summary>
    /// The exception that is thrown when the literal in bear markup language has an invalid format.
    /// </summary>
    public InvalidFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage;

/// <summary>
/// The exception that is thrown when key cannot be found.
/// </summary>
public class KeyNotFoundException : Exception
{
    /// <summary>
    /// The exception that is thrown when key cannot be found.
    /// </summary>
    public KeyNotFoundException() : base() { }

    /// <summary>
    /// The exception that is thrown when key cannot be found.
    /// </summary>
    public KeyNotFoundException(string message) : base(message) { }

    /// <summary>
    /// The exception that is thrown when key cannot be found.
    /// </summary>
    public KeyNotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}
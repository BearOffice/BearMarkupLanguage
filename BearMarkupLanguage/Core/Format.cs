using System;
using System.Collections.Generic;
using System.Text;

namespace BearMarkupLanguage.Core;

/// <summary>
/// Control the style of BearML config files.
/// </summary>
public static class Format
{
    /// <summary>
    /// Style for elements to print.
    /// </summary>
    public static PrintMode PrintMode { get; set; } = PrintMode.Auto;
    /// <summary>
    /// Maximum number of elements that can be expanded. Affect only in automatic print mode.
    /// </summary>
    public static int MaxElementsNumber { get; set; } = 12;
}

/// <summary>
/// Style for elements to print.
/// </summary>
public enum PrintMode
{
    /// <summary>
    /// Auto.
    /// </summary>
    Auto,
    /// <summary>
    /// Compact.
    /// </summary>
    Compact,
    /// <summary>
    /// Expand.
    /// </summary>
    Expand
}

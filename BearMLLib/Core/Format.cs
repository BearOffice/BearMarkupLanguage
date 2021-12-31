using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core
{
    /// <summary>
    /// Control the config file's style.
    /// </summary>
    public class Format
    {
        /// <summary>
        /// Get the <see cref="Format"/>'s instance.
        /// </summary>
        public static Format This { get; } = new Format();
        /// <summary>
        /// This property control escaped value and literial value's style.
        /// </summary>
        public ValueMode ValueMode { get; set; } = ValueMode.Auto;
        /// <summary>
        /// This property control expanded list and folded list's style.
        /// </summary>
        public ListMode ListMode { get; set; } = ListMode.Auto;
        /// <summary>
        /// This property control the border between expanded list and folded list's style.
        /// If the list elements' number are larger than the set value, it will be folded.
        /// </summary>
        public int MaximumElementNumber { get; set; } = 10;
        
        private Format() { }
    }

    /// <summary>
    /// Modes of value.
    /// </summary>
    public enum ValueMode
    {
        /// <summary>
        /// Force value to be escaped.
        /// </summary>
        ForceEscaped,
        /// <summary>
        /// Force value to be literial.
        /// </summary>
        ForceLiterial,
        /// <summary>
        /// Choose the style of value automatically.
        /// </summary>
        Auto
    }

    /// <summary>
    /// Modes of List.
    /// </summary>
    public enum ListMode
    {
        /// <summary>
        /// Force list to be expanded.
        /// </summary>
        ForceExpanded,
        /// <summary>
        /// Force list to be folded.
        /// </summary>
        ForceFolded,
        /// <summary>
        /// Choose the style of list automatically.
        /// </summary>
        Auto
    }
}

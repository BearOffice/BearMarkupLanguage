using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core
{
    public class Format
    {
        public static Format This { get; } = new Format();
        public ValueMode ValueMode { get; set; } = ValueMode.Auto;
        public ListMode ListMode { get; set; } = ListMode.Auto;
        public int MaximumElementNumber { get; set; } = 10;
        
        private Format() { }
    }

    public enum ValueMode
    {
        ForceEscaped,
        ForceLiterial,
        Auto
    }

    public enum ListMode
    {
        ForceExpanded,
        ForceFolded,
        Auto
    }
}

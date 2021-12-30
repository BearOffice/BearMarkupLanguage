using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core.Helpers
{
    internal struct TaggedLine
    {
        internal bool IsItemName { get; }
        internal string Text { get; }

        internal TaggedLine(bool isItemName, string text)
        {
            IsItemName = isItemName;
            Text = text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Text
{
    internal enum EscapeLevel
    {
        L,   // low
        M,   // Middle

        HG,  // high (group)
        HK,  // high (key)
        HC,  // gigh (content)
        HL   // high (list)
    }
}

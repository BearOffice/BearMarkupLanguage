using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreSerializationAttribute : Attribute { }
}

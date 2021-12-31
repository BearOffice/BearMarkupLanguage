using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization
{
    /// <summary>
    /// Indicates that a field or property should not be serialized or deserialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreSerializationAttribute : Attribute { }
}

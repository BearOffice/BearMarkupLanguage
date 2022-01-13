using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization
{
    /// <summary>
    /// Indicates a field or property's key alias.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class KeyAliasAttribute : Attribute
    {
        /// <summary>
        /// Key alias.
        /// </summary>
        public string KeyAlias { get; }

        /// <summary>
        /// Create a keyalias attribute.
        /// </summary>
        public KeyAliasAttribute(string keyalias)
        {
            KeyAlias = keyalias;
        }
    }
}

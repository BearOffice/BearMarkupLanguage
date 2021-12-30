using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib
{
    /// <summary>
    /// ConfigNotMatchException
    /// </summary>
    public class ConfigNotMatchException : Exception
    {
        /// <summary>
        /// ConfigNotMatchException
        /// </summary>
        public ConfigNotMatchException() : base() { }

        /// <summary>
        /// ConfigNotMatchException
        /// </summary>
        public ConfigNotMatchException(string message) : base(message) { }

        /// <summary>
        /// ConfigNotMatchException
        /// </summary>
        public ConfigNotMatchException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
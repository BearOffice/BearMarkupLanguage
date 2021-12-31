using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib
{
    /// <summary>
    /// The exception that is thrown when congig cannot be matched.
    /// </summary>
    public class ConfigNotMatchException : Exception
    {
        /// <summary>
        /// The exception that is thrown when congig cannot be matched.
        /// </summary>
        public ConfigNotMatchException() : base() { }

        /// <summary>
        /// The exception that is thrown when congig cannot be matched.
        /// </summary>
        public ConfigNotMatchException(string message) : base(message) { }

        /// <summary>
        /// The exception that is thrown when congig cannot be matched.
        /// </summary>
        public ConfigNotMatchException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
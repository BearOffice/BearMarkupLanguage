using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Configs
{
    /// <summary>
    /// The exception that is thrown when congig is invalid.
    /// </summary>
    public class InvalidConfigException : Exception
    {
        /// <summary>
        /// The exception that is thrown when congig is invalid.
        /// </summary>
        public InvalidConfigException() : base() { }

        /// <summary>
        /// The exception that is thrown when congig is invalid.
        /// </summary>
        public InvalidConfigException(string message) : base(message) { }

        /// <summary>
        /// The exception that is thrown when congig is invalid.
        /// </summary>
        public InvalidConfigException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
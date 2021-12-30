using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Configs
{
    /// <summary>
    /// InvalidConfigException
    /// </summary>
    public class InvalidConfigException : Exception
    {
        /// <summary>
        /// InvalidConfigException
        /// </summary>
        public InvalidConfigException() : base() { }

        /// <summary>
        /// InvalidConfigException
        /// </summary>
        public InvalidConfigException(string message) : base(message) { }

        /// <summary>
        /// InvalidConfigException
        /// </summary>
        public InvalidConfigException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
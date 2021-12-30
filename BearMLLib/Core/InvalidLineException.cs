using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core
{
    /// <summary>
    /// InvalidLineException
    /// </summary>
    public class InvalidLineException : Exception
    {
        /// <summary>
        /// InvalidLineException
        /// </summary>
        public InvalidLineException() : base() { }

        /// <summary>
        /// InvalidLineException
        /// </summary>
        public InvalidLineException(string message) : base(message) { }

        /// <summary>
        /// InvalidLineException
        /// </summary>
        public InvalidLineException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}

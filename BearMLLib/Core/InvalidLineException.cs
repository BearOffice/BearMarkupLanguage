using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core
{
    /// <summary>
    /// The exception that is thrown when line is invalid.
    /// </summary>
    public class InvalidLineException : Exception
    {
        /// <summary>
        /// The exception that is thrown when line is invalid.
        /// </summary>
        public InvalidLineException() : base() { }

        /// <summary>
        /// The exception that is thrown when line is invalid.
        /// </summary>
        public InvalidLineException(string message) : base(message) { }

        /// <summary>
        /// The exception that is thrown when line is invalid.
        /// </summary>
        public InvalidLineException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}

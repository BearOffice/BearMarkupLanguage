using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization
{
    /// <summary>
    /// TypeNotSupportException
    /// </summary>
    public class TypeNotSupportException : Exception
    {
        /// <summary>
        /// TypeNotSupportException
        /// </summary>
        public TypeNotSupportException() : base() { }

        /// <summary>
        /// TypeNotSupportException
        /// </summary>
        public TypeNotSupportException(string message) : base(message) { }

        /// <summary>
        /// TypeNotSupportException
        /// </summary>
        public TypeNotSupportException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}

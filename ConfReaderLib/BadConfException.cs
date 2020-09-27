using System;
using System.Collections.Generic;
using System.Text;

namespace ConfReaderLib
{
    /// <summary>
    /// BadConfException
    /// </summary>
    public class BadConfException : Exception
    {
        /// <summary>
        /// BadConfException
        /// </summary>
        public BadConfException() : base()
        {

        }
        /// <summary>
        /// BadConfException
        /// </summary>
        public BadConfException(string message) : base(message)
        {

        }
        /// <summary>
        /// BadConfException
        /// </summary>
        public BadConfException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

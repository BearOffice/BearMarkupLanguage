using System;

namespace ConfigReadingLib
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

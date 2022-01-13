using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Serialization
{
    /// <summary>
    /// Indicates a field or property's comment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CommentAttribute : Attribute 
    {
        /// <summary>
        /// Comment.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Create a comment attribute.
        /// </summary>
        public CommentAttribute(string comment)
        {
            Comment = comment;
        }
    }
}

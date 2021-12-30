using System;
using System.Collections.Generic;
using System.Text;

namespace BearMLLib.Core.Helpers
{
    internal class ErrorHandler
    {
        public static ErrorHandler This { get; } = new ErrorHandler();
        public event Action<ErrorArgs> Message;

        private ErrorHandler() { }

        public void Add(ErrorType errorType, int lineOffset, string message)
        {
            Message?.Invoke(new ErrorArgs(errorType, lineOffset, message));
        }

        public void Add(ErrorType errorType, string message)
        {
            Message?.Invoke(new ErrorArgs(errorType, 0, message));
        }

        public void Add(ErrorType errorType)
        {
            Message?.Invoke(new ErrorArgs(errorType, 0, ""));
        }
    }

    internal class ErrorArgs
    {
        public ErrorType Type { get; }
        public int LineOffset { get; }
        public string Message { get; }

        public ErrorArgs(ErrorType errorType, int lineOffset, string message)
        {
            Type = errorType;
            LineOffset = lineOffset;
            Message = message;
        }
    }

    internal enum ErrorType
    {
        TypeNotSupport,
        InvalidConfig,
        InvalidLine,
        NotMatch
    }
}

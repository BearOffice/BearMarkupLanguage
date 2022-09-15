using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BearMarkupLanguage.Helpers;

internal class ExceptionArgs
{
    public ExceptionType Type { get; }
    public int LineOffset { get; }
    public string Message { get; }

    public ExceptionArgs(ExceptionType errorType, int lineOffset, string message)
    {
        Type = errorType;
        LineOffset = lineOffset;
        Message = message;
    }
}

internal enum ExceptionType
{
    TypeNotSupport,
    InvalidConfig,
    InvalidLine,
    NotMatch
}

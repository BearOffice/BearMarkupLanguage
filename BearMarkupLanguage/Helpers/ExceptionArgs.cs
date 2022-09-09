using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

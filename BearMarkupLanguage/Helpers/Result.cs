using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BearMarkupLanguage.Helpers;

internal class Result<TSuccess, TFail>
{
    private readonly TSuccess _success;
    private readonly TFail _fail;
    internal TSuccess Value { get => _success; }
    internal TFail Error { get => _fail; }
    internal bool IsSuccess { get => _success is not null; }
    
    internal Result(TSuccess success)
    {
        _success = success;
        _fail = default;
    }

    internal Result(TFail fail)
    {
        _success = default;
        _fail = fail;
    }
}

internal static class Result
{
    internal static Result<TSuccess, TFail> Success<TSuccess, TFail>(TSuccess success)
    {
        return new Result<TSuccess, TFail>(success);
    }

    internal static Result<TSuccess, TFail> Fail<TSuccess, TFail>(TFail fail)
    {
        return new Result<TSuccess, TFail>(fail);
    }
}

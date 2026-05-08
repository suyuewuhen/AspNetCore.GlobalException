using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Abstractions
{
    public interface IExceptionResponseFormatter
    {
        Task FormatResponseAsync(HttpContext context, 
            int errorCode,
            string message,
            string? traceId,
            string? stackTrace, ExceptionHandlingOptions options);
    }
}

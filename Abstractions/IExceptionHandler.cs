using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Abstractions
{
    public interface IExceptionHandler
    {
        int Order { get; }
        bool CanHandle(Exception ex);

        Task HandleAsync(HttpContext context, Exception exception, ExceptionHandlingOptions options);
    }
}

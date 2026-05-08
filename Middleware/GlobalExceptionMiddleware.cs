using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Middleware
{
    internal class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ExceptionHandlingOptions _options;
        private readonly IEnumerable<IExceptionHandler> _exceptionHandlers;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            IOptions<ExceptionHandlingOptions> options,
            IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            _next = next;
            _options = options.Value;
            _exceptionHandlers = exceptionHandlers.OrderByDescending(h => h.Order);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 如果请求路径在排除路径列表中，则直接调用下一个中间件
            if (_options.ExcludePaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            try
            {
                await _next(context);

            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    throw; // 如果响应已经开始，无法修改响应内容，直接抛出异常
                }

                if(_options.OnExceptionBeforeHandle != null)
                {
                    await _options.OnExceptionBeforeHandle(context, ex);
                }

                var handler = _exceptionHandlers.FirstOrDefault(h => h.CanHandle(ex)) 
                    ?? throw new InvalidOperationException("No exception handler found for the current exception. Ensure DefaultExceptionHandler is registered.");

                await handler.HandleAsync(context, ex, _options);

                if(_options.OnExceptionAfterHandle != null)
                {
                    await _options.OnExceptionAfterHandle(context, ex);
                }
            }
        }
    }
}

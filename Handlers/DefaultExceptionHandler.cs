using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.GlobalException.Exceptions;
using AspNetCore.GlobalException.Constants;

namespace AspNetCore.GlobalException.Handlers
{
    internal class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<DefaultExceptionHandler> _logger;
        private readonly IExceptionResponseFormatter _responseFormatter;
        private readonly IHostingEnvironment _hostEnvironment;

        public DefaultExceptionHandler(
            ILogger<DefaultExceptionHandler> logger,
            IExceptionResponseFormatter responseFormatter,
            IHostingEnvironment hostEnvironment)
        {
            _logger = logger;
            _responseFormatter = responseFormatter;
            _hostEnvironment = hostEnvironment;
        }
        public int Order => int.MinValue;

        public bool CanHandle(Exception ex) => true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task HandleAsync(HttpContext context, Exception exception, ExceptionHandlingOptions options)
        {
            var traceId = options.EnableTracing ? context.TraceIdentifier : null;
            var isDevelopment = _hostEnvironment.IsDevelopment();
            var includeStackTrace = options.IncludeStackTrace || isDevelopment;
            var (errorCode, message, logLevel) = ResolveExceptionInfo(exception, options, isDevelopment);
            WriteStructuredLog(context, exception, traceId, logLevel);
            await _responseFormatter.FormatResponseAsync(context, errorCode, message, traceId, includeStackTrace ? exception.StackTrace : null,options);
        }

        
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <param name="traceId"></param>
        /// <param name="logLevel"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void WriteStructuredLog(HttpContext context, Exception exception, string? traceId, object logLevel)
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                [ExceptionDefaults.TraceIdKey] = traceId ?? string.Empty,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
                ["ExceptionType"] = exception.GetType().Name,
                ["UserIdentity"] = context.User.Identity?.Name ?? string.Empty
            });

            _logger.Log((LogLevel)logLevel, exception, "全局异常捕获：{Message}" , exception.Message);
        }

        /// <summary>
        /// 解析异常：错误码 + 提示信息 + 日志级别
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="options"></param>
        /// <param name="isDevelopment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private (int errorCode, string message, object logLevel) ResolveExceptionInfo(Exception exception, ExceptionHandlingOptions options, bool isDevelopment)
        {
            if(exception is GlobalExceptionBase baseException)
            {
                var logLevel = options.ExceptionLogLevelMapping.TryGetValue(exception.GetType(), out var level) ? level : LogLevel.Warning;

                var message = baseException.IsUserFriendly || isDevelopment
                    ? baseException.Message
                    : options.ProductionErrorMessage;

                return (baseException.ErrorCode, message, logLevel);
            }

            var defaultLogLevel = options.ExceptionLogLevelMapping.TryGetValue(typeof(Exception), out var defaultLevel) ? defaultLevel : LogLevel.Error;

            var defaultessage = isDevelopment ? exception.Message : options.ProductionErrorMessage;
            
            return (ExceptionDefaults.DefaultSystemErrorCode, defaultessage, defaultLogLevel);
        }

    }
}

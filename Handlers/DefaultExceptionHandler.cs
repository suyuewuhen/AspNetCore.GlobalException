using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.GlobalException.Exceptions;
using AspNetCore.GlobalException.Constants;

namespace AspNetCore.GlobalException.Handlers
{
    internal class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<DefaultExceptionHandler> _logger;
        private readonly IExceptionResponseFormatter _responseFormatter;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DefaultExceptionHandler(
            ILogger<DefaultExceptionHandler> logger,
            IExceptionResponseFormatter responseFormatter,
            IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _responseFormatter = responseFormatter;
            _hostEnvironment = hostEnvironment;
        }
        public int Order => int.MinValue;

        public bool CanHandle(Exception ex) => true;

        /// <summary>
        /// 异步处理异常
        /// </summary>
        /// <param name="context">当前Http上下文</param>
        /// <param name="exception">捕获到的异常对象</param>
        /// <param name="options">全局异常处理配置选项</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task HandleAsync(HttpContext context, Exception exception, ExceptionHandlingOptions options)
        {
            var traceId = options.EnableTracing ? context.TraceIdentifier : null;
            var isDevelopment = _hostEnvironment.IsDevelopment();
            var includeStackTrace = options.IncludeStackTrace || isDevelopment;
            var (errorCode, message, logLevel) = ResolveExceptionInfo(exception, options, isDevelopment);
            await WriteStructuredLog(context, exception, traceId, logLevel, options.SensitiveFieldNames);
            await _responseFormatter.FormatResponseAsync(context, errorCode, message, traceId, includeStackTrace ? exception.StackTrace : null,options);
        }

        
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="context">当前Http上下文</param>
        /// <param name="exception">捕获到的异常对象</param>
        /// <param name="traceId">链路追踪ID，可为空</param>
        /// <param name="logLevel">日志级别</param>
        /// <param name="sensitiveFieldNames">敏感字段名称列表</param>
        /// <returns>表示异步操作的任务</returns>
        private async Task WriteStructuredLog(HttpContext context, Exception exception, string? traceId, LogLevel logLevel, List<string> sensitiveFieldNames)
        {
            // 读取请求参数并脱敏
            var requestParams = await GetRequestParametersAsync(context, sensitiveFieldNames);
            
            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                [ExceptionDefaults.TraceIdKey] = traceId ?? string.Empty,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
                ["RequestParameters"] = requestParams,
                ["ExceptionType"] = exception.GetType().Name,
                ["UserIdentity"] = context.User.Identity?.Name ?? string.Empty
            });

            _logger.Log(logLevel, exception, "全局异常捕获：{Message}" , exception.Message);
        }

        /// <summary>
        /// 获取并脱敏请求参数
        /// </summary>
        private async Task<Dictionary<string, object>> GetRequestParametersAsync(HttpContext context, List<string> sensitiveFieldNames)
        {
            var parameters = new Dictionary<string, object>();
            
            // 处理Query参数
            foreach (var (key, value) in context.Request.Query)
            {
                parameters[key] = MaskSensitiveValue(key, value.ToString(), sensitiveFieldNames);
            }

            // 处理Form参数
            if (context.Request.HasFormContentType)
            {
                foreach (var (key, value) in context.Request.Form)
                {
                    parameters[key] = MaskSensitiveValue(key, value.ToString(), sensitiveFieldNames);
                }
            }

            // 处理JSON Body参数
            if (context.Request.ContentType?.Contains("application/json") == true)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var bodyContent = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                
                if (!string.IsNullOrEmpty(bodyContent))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(bodyContent);
                        ExtractJsonParameters(jsonDoc.RootElement, parameters, sensitiveFieldNames, string.Empty);
                    }
                    catch
                    {
                        // 解析失败直接存原始内容（不脱敏，避免丢失信息）
                        parameters["RequestBody"] = bodyContent;
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// 递归提取JSON参数并脱敏
        /// </summary>
        private void ExtractJsonParameters(JsonElement element, Dictionary<string, object> parameters, List<string> sensitiveFieldNames, string prefix)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var fullKey = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        ExtractJsonParameters(property.Value, parameters, sensitiveFieldNames, fullKey);
                    }
                    break;
                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        var fullKey = $"{prefix}[{index}]";
                        ExtractJsonParameters(item, parameters, sensitiveFieldNames, fullKey);
                        index++;
                    }
                    break;
                default:
                    var value = element.ToString();
                    var key = prefix.Split('.').Last(); // 取最后一级作为字段名判断是否敏感
                    parameters[prefix] = MaskSensitiveValue(key, value, sensitiveFieldNames);
                    break;
            }
        }

        /// <summary>
        /// 脱敏敏感字段值
        /// </summary>
        private string MaskSensitiveValue(string fieldName, string value, List<string> sensitiveFieldNames)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(value))
                return value;

            // 检查是否是敏感字段
            if (sensitiveFieldNames.Any(f => fieldName.Equals(f, StringComparison.OrdinalIgnoreCase)))
            {
                if (value.Length <= 2)
                    return "**";
                // 保留前1后1，中间用*代替
                return $"{value[0]}***{value[^1]}";
            }

            return value;
        }

        /// <summary>
        /// 解析异常：错误码 + 提示信息 + 日志级别
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="options"></param>
        /// <param name="isDevelopment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private (int errorCode, string message, LogLevel logLevel) ResolveExceptionInfo(Exception exception, ExceptionHandlingOptions options, bool isDevelopment)
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

            var defaultMessage = isDevelopment ? exception.Message : options.ProductionErrorMessage;
            
            return (ExceptionDefaults.DefaultSystemErrorCode, defaultMessage, defaultLogLevel);
        }

    }
}

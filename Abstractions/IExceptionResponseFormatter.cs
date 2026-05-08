using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Abstractions
{
    /// <summary>
    /// 异常响应格式化器接口，可实现此接口自定义异常返回的响应格式
    /// </summary>
    public interface IExceptionResponseFormatter
    {
        /// <summary>
        /// 异步格式化异常响应
        /// </summary>
        /// <param name="context">当前Http上下文</param>
        /// <param name="errorCode">错误码</param>
        /// <param name="message">错误信息</param>
        /// <param name="traceId">链路追踪ID，可为空</param>
        /// <param name="stackTrace">异常堆栈信息，可为空</param>
        /// <param name="options">全局异常处理配置选项</param>
        /// <returns>表示异步操作的任务</returns>
        Task FormatResponseAsync(HttpContext context, 
            int errorCode,
            string message,
            string? traceId,
            string? stackTrace, ExceptionHandlingOptions options);
    }
}

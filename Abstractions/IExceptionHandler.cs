using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Abstractions
{
    /// <summary>
    /// 异常处理器接口，可实现此接口自定义特定类型异常的处理逻辑
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// 处理器执行顺序，值越大越先执行
        /// </summary>
        int Order { get; }
        
        /// <summary>
        /// 判断当前处理器是否可以处理指定的异常
        /// </summary>
        /// <param name="ex">要处理的异常对象</param>
        /// <returns>如果可以处理返回true，否则返回false</returns>
        bool CanHandle(Exception ex);

        /// <summary>
        /// 异步处理异常
        /// </summary>
        /// <param name="context">当前Http上下文</param>
        /// <param name="exception">捕获到的异常对象</param>
        /// <param name="options">全局异常处理配置选项</param>
        /// <returns>表示异步操作的任务</returns>
        Task HandleAsync(HttpContext context, Exception exception, ExceptionHandlingOptions options);
    }
}

using AspNetCore.GlobalException.Constants;
using AspNetCore.GlobalException.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Options
{
    /// <summary>
    /// 配置选项类，用于全局异常处理中配置相关选项
    /// </summary>
    public class ExceptionHandlingOptions
    {
        /// <summary>
        /// 是否记录异常堆栈信息
        /// </summary>
        public bool IncludeStackTrace { get; set; }
        /// <summary>
        ///默认值 = ExceptionDefaults 里定义的默认提示（比如："服务器异常，请稍后重试"）生产环境不能暴露真实错误，就用这个文案返回给前端
        /// </summary>
        public string ProductionErrorMessage { get; set; } = ExceptionDefaults.DefaultProductionErrorMessage;
        /// <summary>
        /// 排除记录日志的路径列表，默认值 = ExceptionDefaults 里定义的默认排除路径（比如："/health"）可以在这里添加一些不需要记录日志的接口路径，比如健康检查接口等，以减少日志噪音
        /// </summary>
        public List<string> ExcludePaths { get; set; } = ExceptionDefaults.DefaultExcludePaths.ToList();
        /// <summary>
        /// 敏感字段名称列表，默认值 = ExceptionDefaults 里定义的默认敏感字段名称（比如："password"）可以在这里添加一些敏感字段名称，
        /// </summary>
        public List<string> SensitiveFieldNames { get; set; } = ExceptionDefaults.SensitiveFieldNames.ToList();
        /// <summary>
        /// 异常日志级别映射，默认值 = ExceptionDefaults 里定义的默认映射（比如：{ typeof(BusinessException), LogLevel.Warning }）可以在这里添加，不同异常，记录不同等级的日志，方便日志筛选、告警、排查
        /// </summary>
        public Dictionary<Type, LogLevel> ExceptionLogLevelMapping { get; set; } =
            new()
            {
                { typeof(BusinessException), LogLevel.Warning },
                { typeof(ValidationException), LogLevel.Warning },
                { typeof(NotFoundException), LogLevel.Information },
                { typeof(AuthorizationException), LogLevel.Information },
                { typeof(ForbiddenException), LogLevel.Warning },
                { typeof(ThirdPartyServiceException), LogLevel.Error },
                { typeof(Exception), LogLevel.Critical }
            };
        /// <summary>
        /// 异常处理前执行的委托
        /// </summary>
        public Func<HttpContext, Exception, Task>? OnExceptionBeforeHandle { get; set; }
        /// <summary>
        /// 异常处理后执行的委托
        /// </summary>
        public Func<HttpContext, Exception, Task>? OnExceptionAfterHandle { get; set; }
        /// <summary>
        /// 开启后：异常会自动带上 TraceId，方便分布式系统定位请求
        /// </summary>
        public bool EnableTracing { get; set; } = true;

        /// <summary>
        /// 自定义敏感字段脱敏函数
        /// 输入参数：字段名、原始值
        /// 返回值：脱敏后的值
        /// 默认实现：值长度<=2返回**，否则保留首尾各1位，中间用***代替
        /// </summary>
        public Func<string, string, string> SensitiveValueMasker { get; set; } = (fieldName, value) =>
        {
            if (value.Length <= 2)
                return "**";
            return $"{value[0]}***{value[^1]}";
        };
    }
}

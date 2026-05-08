using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Formatters;
using AspNetCore.GlobalException.Handlers;
using AspNetCore.GlobalException.Middleware;
using AspNetCore.GlobalException.Options;


namespace AspNetCore.GlobalException.Extensions;

public static class GlobalExceptionExtensions
{
    /// <summary>
    /// 注册全局异常处理核心服务
    /// </summary>
    public static IServiceCollection AddGlobalException(this IServiceCollection services, Action<ExceptionHandlingOptions>? configure = null)
    {
        // 绑定配置选项
        if (configure is not null)
        {
            services.Configure(configure);
        }

        // 注册核心服务
        services.AddSingleton<IExceptionResponseFormatter, DefaultExceptionResponseFormatter>();
        services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

        return services;
    }

    /// <summary>
    /// 注册自定义异常处理器
    /// </summary>
    public static IServiceCollection AddExceptionHandler<THandler>(this IServiceCollection services)
        where THandler : class, IExceptionHandler
    {
        services.AddSingleton<IExceptionHandler, THandler>();
        return services;
    }

    /// <summary>
    /// 注册自定义响应格式化器
    /// </summary>
    public static IServiceCollection AddExceptionResponseFormatter<TFormatter>(this IServiceCollection services)
        where TFormatter : class, IExceptionResponseFormatter
    {
        services.AddSingleton<IExceptionResponseFormatter, TFormatter>();
        return services;
    }

    /// <summary>
    /// 启用全局异常中间件
    /// </summary>
    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
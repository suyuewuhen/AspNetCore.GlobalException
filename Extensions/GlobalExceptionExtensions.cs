using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Formatters;
using AspNetCore.GlobalException.Handlers;
using AspNetCore.GlobalException.Middleware;
using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Extensions
{
    internal static class GlobalExceptionExtensions
    {
        /// <summary>
        /// 添加全局异常处理服务,扩展方法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddGlobalException(this IServiceCollection services, Action<ExceptionHandlingOptions>? configure = null)
        {
            if (configure is not null)
            {
                services.Configure(configure);
            }

            services.AddSingleton<IExceptionResponseFormatter, DefaultExceptionResponseFormatter>();
            services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

            return services;
        }

        public static IServiceCollection AddExceptionHandler<THandler>(this IServiceCollection services)
            where THandler : class, IExceptionHandler
        {
            services.AddSingleton<IExceptionHandler, THandler>();
            return services;
        }

        public static IServiceCollection AddExceptionResponseFormatter<TFormatter>(this IServiceCollection services)
            where TFormatter : class, IExceptionResponseFormatter
        {
            services.AddSingleton<IExceptionResponseFormatter, TFormatter>();
            return services;
        }

        public static IApplicationBuilder UseGlobalException(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
            return app;
        }
    }
}

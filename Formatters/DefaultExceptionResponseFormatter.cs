using AspNetCore.GlobalException.Abstractions;
using AspNetCore.GlobalException.Models;
using AspNetCore.GlobalException.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetCore.GlobalException.Formatters
{
    internal class DefaultExceptionResponseFormatter : IExceptionResponseFormatter
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public async Task FormatResponseAsync(HttpContext context, int errorCode, string message, string? traceId, string? stackTrace, ExceptionHandlingOptions options)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = 200;
            var response = ApiResponse<object>.Fail(errorCode, message, traceId, stackTrace);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));

        }
    }
}

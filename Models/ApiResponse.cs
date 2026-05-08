using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Models
{
    internal class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Msg { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? TraceId { get; set; }
        public string? StackTrace { get; set; }

        public static ApiResponse<T> Fail(int code, string msg, string? traceId = null, string? stackTrace = null)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Msg = msg,
                TraceId = traceId,
                StackTrace = stackTrace
            };
        }
    }
}

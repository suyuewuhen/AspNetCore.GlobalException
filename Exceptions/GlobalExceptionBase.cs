using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{

    /// <summary>
    /// 全局异常基类
    /// </summary>
    public abstract class GlobalExceptionBase:Exception
    {
        public int ErrorCode { get; }
        public string? TraceId { get; }
        public bool IsUserFriendly { get; }

        protected GlobalExceptionBase(string message, int errorCode, string? traceId = null, bool isUserFriendly = true,Exception? exception = null):base(message,exception)
        {
            ErrorCode = errorCode;
            //TraceId = traceId;
            IsUserFriendly = isUserFriendly;

        }
    }
}

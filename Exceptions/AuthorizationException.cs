using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{
    /// <summary>
    /// 授权异常
    /// </summary>
    internal class AuthorizationException : GlobalExceptionBase
    {
        public AuthorizationException(string message = "未授权访问", int errorCode = ExceptionDefaults.DefaultAuthErrorCode, string? traceId = null, bool isUserFriendly = true, Exception? exception = null) : base(message, errorCode, traceId, isUserFriendly, exception)
        {
        }
    }
}

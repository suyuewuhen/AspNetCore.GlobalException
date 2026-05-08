using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{
    internal class ForbiddenException : GlobalExceptionBase
    {
        public ForbiddenException(string message = "无权限执行此操作", int errorCode = ExceptionDefaults.DefaultForbiddenErrorCode, string? traceId = null, bool isUserFriendly = true, Exception? exception = null) : base(message, errorCode, traceId, isUserFriendly, exception)
        {
        }
    }
}

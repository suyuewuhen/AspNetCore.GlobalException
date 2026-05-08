using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{
    /// <summary>
    /// Not Found Exception
    /// </summary>
    public class NotFoundException : GlobalExceptionBase
    {
        public NotFoundException(string resourceName, int errorCode = ExceptionDefaults.DefaultNotFoundErrorCode, string? traceId = null, bool isUserFriendly = true, Exception? exception = null) : base($"资源 {resourceName} 不存在", errorCode, traceId, isUserFriendly, exception)
        {
        }
    }
}

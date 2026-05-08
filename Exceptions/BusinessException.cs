using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{
    /// <summary>
    /// BusinessException
    /// </summary>
    public class BusinessException : GlobalExceptionBase
    {
        public BusinessException(string message,
            int errorCode = ExceptionDefaults.DefaultBusinessErrorCode, 
            string? traceId = null, 
            bool isUserFriendly = true, 
            Exception? exception = null) : base(message, errorCode, traceId, isUserFriendly, exception)
        {
        }
    }
}

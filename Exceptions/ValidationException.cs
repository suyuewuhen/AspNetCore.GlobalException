using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{

    /// <summary>
    /// Validation Exception
    /// </summary>
    internal class ValidationException : GlobalExceptionBase
    {
        public ValidationException(string message, 
            int errorCode = ExceptionDefaults.DefaultValidationErrorCode, 
            string? traceId = null, 
            bool isUserFriendly = true, 
            Exception? exception = null) : base(message, errorCode, traceId, isUserFriendly, exception)
        {
        }
    }
}

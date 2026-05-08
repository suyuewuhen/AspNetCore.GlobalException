using AspNetCore.GlobalException.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Exceptions
{
    public class ThirdPartyServiceException : GlobalExceptionBase

    {
        public string ServiceName { get; }
        public ThirdPartyServiceException(string serviceName, string message = "第三方服务异常", int errorCode = ExceptionDefaults.DefaultThirdPartyErrorCode, string? traceId = null, bool isUserFriendly = true, Exception? exception = null) : base(message, errorCode, traceId, isUserFriendly, exception)
        {
            ServiceName = serviceName;
        }
    }
}

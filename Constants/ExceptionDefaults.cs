using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.GlobalException.Constants
{
    public class ExceptionDefaults
    {
        public const int DefaultBusinessErrorCode = 400;
        public const int DefaultSystemErrorCode = 500;
        public const int DefaultAuthErrorCode = 401;
        public const int DefaultForbiddenErrorCode = 403;
        public const int DefaultNotFoundErrorCode = 404;
        public const int DefaultValidationErrorCode = 400;
        public const int DefaultThirdPartyErrorCode = 502;

        public const string DefaultProductionErrorMessage = "服务器内部错误，请联系管理员";
        public const string TraceIdKey = "TraceId";
        public static readonly string[] DefaultExcludePaths = ["/healthz", "/health", "/swagger", "/favicon.ico"];
        public static readonly string[] SensitiveFieldNames = ["password", "pwd", "token", "apikey", "secret", "phone", "idcard"];

    }
}

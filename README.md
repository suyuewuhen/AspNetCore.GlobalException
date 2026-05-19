# AspNetCore.GlobalException 全局异常处理组件

一个企业级、高度可扩展的ASP.NET Core全局异常处理中间件，统一异常返回格式、自动日志记录、敏感字段脱敏，开箱即用。

## 功能特性

✅ **统一异常处理**：全局捕获所有未处理异常，统一返回标准JSON格式
✅ **多环境适配**：开发环境返回详细错误信息，生产环境隐藏敏感细节
✅ **内置异常类型**：提供常用业务异常类型（业务异常、验证异常、未授权、未找到、第三方服务异常等）
✅ **可扩展架构**：支持自定义异常处理器、自定义响应格式化器
✅ **敏感字段脱敏**：自动脱敏请求参数中的敏感字段（密码、Token、手机号、身份证等）
✅ **链路追踪**：内置TraceId支持，方便分布式系统问题定位
✅ **结构化日志**：自动记录请求上下文信息，支持日志聚合分析
✅ **高度可配置**：支持排除路径、日志级别映射、自定义错误信息等

## 快速开始

### 1. 安装NuGet包

```bash
Install-Package AspNetCore.GlobalException
```

### 2. 注册服务

在 `Program.cs` 中添加服务注册：

```csharp
builder.Services.AddGlobalException(options =>
{
    // 可选配置，以下为默认值
    options.IncludeStackTrace = false; // 是否在生产环境返回堆栈信息
    options.ProductionErrorMessage = "服务器内部错误，请联系管理员"; // 生产环境错误提示
    options.EnableTracing = true; // 启用TraceId
    options.ExcludePaths.Add("/custom-health"); // 添加自定义排除路径
    options.SensitiveFieldNames.Add("creditCard"); // 添加自定义敏感字段
});
```

### 3. 启用中间件

在 `Program.cs` 中启用中间件（**建议放在中间件管道最开头**）：

```csharp
var app = builder.Build();

app.UseGlobalException(); // 放在最开头，确保捕获所有中间件的异常

// 其他中间件...
app.UseRouting();
app.UseAuthorization();
// ...
```

## 标准返回格式

```json
{
  "code": 500,
  "msg": "服务器内部错误，请联系管理员",
  "data": null,
  "traceId": "0HM1234567890",
  "stackTrace": null // 开发环境或配置开启时返回
}
```

## 内置异常类型

| 异常类型 | 默认错误码 | 默认日志级别 | 说明 |
|---------|-----------|-------------|------|
| `BusinessException` | 400 | Warning | 业务逻辑异常 |
| `ValidationException` | 400 | Warning | 参数验证异常 |
| `NotFoundException` | 404 | Information | 资源未找到异常 |
| `AuthorizationException` | 401 | Information | 未授权异常 |
| `ForbiddenException` | 403 | Warning | 权限不足异常 |
| `ThirdPartyServiceException` | 502 | Error | 第三方服务调用异常 |
| 其他系统异常 | 500 | Critical | 系统级异常 |

### 使用示例

```csharp
// 抛出业务异常
throw new BusinessException("用户余额不足", 1001); // 自定义错误码1001

// 抛出参数验证异常
throw new ValidationException("手机号格式不正确");

// 抛出资源未找到异常
throw new NotFoundException("用户不存在");
```

## 高级配置

### 自定义异常处理器

1. 实现 `IExceptionHandler` 接口：

```csharp
public class CustomExceptionHandler : IExceptionHandler
{
    public int Order => 100; // 优先级，值越大越先执行

    public bool CanHandle(Exception ex)
    {
        return ex is MyCustomException; // 指定要处理的异常类型
    }

    public async Task HandleAsync(HttpContext context, Exception exception, ExceptionHandlingOptions options)
    {
        // 自定义处理逻辑
        var ex = (MyCustomException)exception;
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            code = ex.ErrorCode,
            message = ex.Message,
            customField = ex.CustomData
        });
    }
}
```

2. 注册自定义处理器：

```csharp
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
```

### 自定义响应格式化器

1. 实现 `IExceptionResponseFormatter` 接口：

```csharp
public class CustomResponseFormatter : IExceptionResponseFormatter
{
    public async Task FormatResponseAsync(HttpContext context, int errorCode, string message, string? traceId, string? stackTrace, ExceptionHandlingOptions options)
    {
        // 自定义返回格式
        var response = new
        {
            status = errorCode,
            error = message,
            requestId = traceId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

2. 注册自定义格式化器：

```csharp
builder.Services.AddExceptionResponseFormatter<CustomResponseFormatter>();
```

### 事件钩子

```csharp
builder.Services.AddGlobalException(options =>
{
    // 异常处理前执行
    options.OnExceptionBeforeHandle = async (context, ex) =>
    {
        // 自定义逻辑，比如上报监控
        await Task.CompletedTask;
    };
    
    // 异常处理后执行
    options.OnExceptionAfterHandle = async (context, ex) =>
    {
        // 自定义逻辑，比如统计异常次数
        await Task.CompletedTask;
    };
});
```

### 自定义敏感字段脱敏策略

```csharp
builder.Services.AddGlobalException(options =>
{
    // 自定义脱敏规则，不同字段采用不同脱敏方式
    options.SensitiveValueMasker = (fieldName, value) =>
    {
        // 手机号：保留前3后4
        if (fieldName.Equals("phone", StringComparison.OrdinalIgnoreCase) && value.Length >= 11)
        {
            return $"{value.Substring(0, 3)}****{value.Substring(7, 4)}";
        }
        // 身份证号：保留前6后4
        if (fieldName.Equals("idcard", StringComparison.OrdinalIgnoreCase) && value.Length >= 18)
        {
            return $"{value.Substring(0, 6)}********{value.Substring(14, 4)}";
        }
        // 邮箱：保留用户名前2位和域名
        if (fieldName.Equals("email", StringComparison.OrdinalIgnoreCase) && value.Contains('@'))
        {
            var parts = value.Split('@');
            return parts[0].Length > 2 ? $"{parts[0].Substring(0, 2)}***@{parts[1]}" : $"***@{parts[1]}";
        }
        // 默认规则：保留首尾各1位，中间用***代替
        return value.Length <= 2 ? "**" : $"{value[0]}***{value[^1]}";
    };
});
```

## 配置选项说明

| 配置项 | 类型 | 默认值 | 说明 |
|-------|------|--------|------|
| `IncludeStackTrace` | `bool` | `false` | 是否在响应中包含堆栈信息，开发环境自动开启 |
| `ProductionErrorMessage` | `string` | "服务器内部错误，请联系管理员" | 生产环境返回的通用错误信息 |
| `ExcludePaths` | `List<string>` | ["/healthz", "/health", "/swagger", "/favicon.ico"] | 不进行异常处理的路径列表 |
| `SensitiveFieldNames` | `List<string>` | ["password", "pwd", "token", "apikey", "secret", "phone", "idcard"] | 需要脱敏的敏感字段列表 |
| `ExceptionLogLevelMapping` | `Dictionary<Type, LogLevel>` | 见内置异常类型表 | 不同异常类型对应的日志级别 |
| `OnExceptionBeforeHandle` | `Func<HttpContext, Exception, Task>?` | `null` | 异常处理前的钩子函数 |
| `OnExceptionAfterHandle` | `Func<HttpContext, Exception, Task>?` | `null` | 异常处理后的钩子函数 |
| `EnableTracing` | `bool` | `true` | 是否在响应中返回TraceId |
| `SensitiveValueMasker` | `Func<string, string, string>` | 默认实现 | 自定义敏感字段脱敏函数，输入为字段名和原始值，返回脱敏后的值 |

## 敏感字段脱敏

组件会自动对以下参数中的敏感字段进行脱敏：
- Query参数
- Form表单参数
- JSON Body参数（支持嵌套对象和数组）

脱敏规则：保留首尾各1位字符，中间用`***`代替，例如：
- 密码：`p***d`
- 手机号：`1***0`
- 身份证号：`1***X`

## License

MIT
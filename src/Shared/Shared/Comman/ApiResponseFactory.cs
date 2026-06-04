using Shared.Models;

namespace Shared.Common;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string message = "Success", int statusCode = 200, string traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            TraceId = traceId
        };
    }

    public static ApiResponse<T> Fail<T>(string message, int statusCode = 400, List<FieldError>? errors = null, string traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new(),
            StatusCode = statusCode,
            TraceId = traceId
        };
    }
}
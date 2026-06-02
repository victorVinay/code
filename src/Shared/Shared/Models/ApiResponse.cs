using System;
using System.Collections.Generic;

namespace Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<FieldError> Errors { get; set; } = new();
        public string TraceId { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class FieldError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}

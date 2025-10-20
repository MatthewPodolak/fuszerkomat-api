using System.ComponentModel.DataAnnotations;
using System.Net;

namespace fuszerkomat_api.Data
{
    public class Result
    {
        public bool Success { get; init; }
        public int Status { get; init; } = (int)HttpStatusCode.OK;
        public string? TraceId { get; init; }
        public List<Error>? Errors { get; init; }

        public static Result Ok(List<Error>? errors, string traceId)
            => new Result() { Errors = errors, Success = true, Status = (int)HttpStatusCode.OK, TraceId = traceId };

        public static Result BadRequest(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.Validation() }, Success = false, Status = (int)HttpStatusCode.BadRequest, TraceId = traceId };

        public static Result NotFound(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.NotFound() }, Success = false, Status = (int)HttpStatusCode.NotFound, TraceId = traceId };

        public static Result Conflict(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.Conflict() }, Success = false, Status = (int)HttpStatusCode.Conflict, TraceId = traceId };

        public static Result Forbidden(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.Forbidden() }, Success = false, Status = (int)HttpStatusCode.Forbidden, TraceId = traceId };

        public static Result Unauthorized(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.Unauthorized() }, Success = false, Status = (int)HttpStatusCode.Unauthorized, TraceId = traceId };

        public static Result Internal(List<Error>? errors, string traceId)
            => new() { Errors = errors ?? new List<Error> { Error.Internal() }, Success = false, Status = (int)HttpStatusCode.InternalServerError, TraceId = traceId };
    }

    public class Result<T> : Result
    {
        public T? Data { get; init; }
        public Pagination? Pagination { get; set; }

        public static Result<T> Ok(string traceId, T? data = default, Pagination? pagination = null)
            => new Result<T>() { Data = data, Pagination = pagination, Status = (int)HttpStatusCode.OK, Success = true, TraceId = traceId };

        public static Result<T> BadRequest(string traceId, T? data = default, Pagination? pagination = null, List<Error>? errors = null)
            => new Result<T>() { Data = data, Pagination = pagination, Status = (int)HttpStatusCode.BadRequest, Success = false, TraceId = traceId, Errors = errors ?? new List<Error>() { Error.Validation() } };

        public static Result<T> NotFound(string traceId, T? data = default, Pagination? pagination = null, List<Error>? errors = null)
            => new Result<T>() { Data = data, Pagination = pagination, Status = (int)HttpStatusCode.NotFound, Success = false, TraceId = traceId, Errors = errors ?? new List<Error>() { Error.NotFound() } };

        public static Result<T> Internal(string traceId, T? data = default, Pagination? pagination = null, List<Error>? errors = null)
            => new Result<T>() { Data = data, Pagination = pagination, Status = (int)HttpStatusCode.InternalServerError, Success = false, TraceId = traceId, Errors = errors ?? new List<Error>() { Error.Internal() } };

        public static Result<T> Canceled(string traceId, T? data = default, Pagination? pagination = null, List<Error>? errors = null)
            => new Result<T>() { Data = data, Pagination = pagination, Status = 499, Success = false, TraceId = traceId, Errors = errors ?? new List<Error>() { Error.Canceled() } };
    }

    public class Pagination
    {
        [Range(1, int.MaxValue)]
        public int CurrentPage { get; set; }
        public int Count { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }

        public bool HasNext => CurrentPage < PageCount;
        public bool HasPrevious => CurrentPage > 1;
    }

    public class Error
    {
        public Error(ErrorCode code, string message, string? detail = null)
        {
            Code = code;
            Message = message;
            Detail = detail;
        }

        public ErrorCode Code { get; init; }
        public string Message { get; init; } = "Something went wrong";
        public string? Detail { get; init; }

        public static Error NotFound(string msg = "Resource not found", string? detail = null)
            => new Error(ErrorCode.NotFound, msg, detail);

        public static Error Conflict(string msg = "Conflict occured", string? detail = null)
            => new Error(ErrorCode.Conflict, msg, detail);

        public static Error Forbidden(string msg = "Operation prohibited", string? detail = null)
            => new Error(ErrorCode.Forbidden, msg, detail);

        public static Error Validation(string msg = "Validation failed", string? detail = null)
            => new Error(ErrorCode.ValidationFailed, msg, detail);

        public static Error Unauthorized(string msg = "Unauthorized", string? detail = null)
            => new Error(ErrorCode.Unauthorized, msg, detail);

        public static Error Internal(string msg = "Internal server error", string? detail = null)
            => new Error(ErrorCode.Internal, msg, detail);
        public static Error Canceled(string msg = "Operation was canceled", string? detail = null)
            => new(ErrorCode.Canceled, msg, detail);
    }

    public enum ErrorCode
    {
        None,
        ValidationFailed,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,
        RateLimited,
        Internal,
        Unexpected,
        Canceled
    }
}

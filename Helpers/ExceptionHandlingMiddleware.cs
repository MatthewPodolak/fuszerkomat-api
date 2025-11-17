using fuszerkomat_api.Data;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static fuszerkomat_api.Helpers.DomainExceptions;

namespace fuszerkomat_api.Helpers
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                string msg = ex.LogMessage ?? "Resource not found";

                _logger.LogWarning(ex, "{Message} Data={Data} Path={Path}, Method={Method}, TraceId={TraceId}",
                    msg,
                    ex.Data,
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound, ErrorCode.NotFound);
            }
            catch(ConflictException ex)
            {
                string msg = ex.LogMessage ?? "Resource not found";

                _logger.LogWarning(ex, "{Message} Data={Data} Path={Path}, Method={Method}, TraceId={TraceId}",
                    msg,
                    ex.Data,
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, ErrorCode.Conflict);
            }
            catch (UnauthorizedException ex)
            {
                string msg = ex.LogMessage ?? "Resource not found";

                _logger.LogWarning(ex, "{Message} Data={Data} Path={Path}, Method={Method}, TraceId={TraceId}",
                    msg,
                    ex.Data,
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, ErrorCode.Unauthorized);
            }
            catch (ForbiddenException ex)
            {
                string msg = ex.LogMessage ?? "Resource not found";

                _logger.LogWarning(ex, "{Message} Data={Data} Path={Path}, Method={Method}, TraceId={TraceId}",
                    msg,
                    ex.Data,
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.Forbidden, ErrorCode.Forbidden);
            }
            catch(InternalException ex)
            {
                string msg = ex.LogMessage ?? "Internal server error.";

                _logger.LogWarning(ex, "{Message} Data={Data} Path={Path}, Method={Method}, TraceId={TraceId}",
                    msg,
                    ex.Data,
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, ErrorCode.Internal);
            }
            catch(ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error. Path={Path}, Method={Method}, TraceId={TraceId}",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, ErrorCode.ValidationFailed);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Request was canceled. Path={Path}, Method={Method}, TraceId={TraceId}",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, (HttpStatusCode)499, ErrorCode.Canceled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. Path={Path}, Method={Method}, TraceId={TraceId}",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);

                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, ErrorCode.Internal);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, HttpStatusCode statusCode, ErrorCode errorCode)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Cannot write error response, response has already started. Path={Path}, Method={Method}, TraceId={TraceId}",
                    context.Request.Path,
                    context.Request.Method,
                    context.TraceIdentifier);
                return;
            }

            var traceId = context.TraceIdentifier;

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var error = new Error(errorCode, ex.Message);

            var result = new Result
            {
                Success = false,
                Status = (int)statusCode,
                TraceId = traceId,
                Errors = new List<Error> { error }         
            };

            await context.Response.WriteAsJsonAsync(result);
        }

    }
}

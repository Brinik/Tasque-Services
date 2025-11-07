using System.ComponentModel.DataAnnotations;

namespace TasqueManager.WebHost.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);

                //Проверки для логгирования при исключении по-умолчанию
                if (httpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    _logger.LogWarning("BadRequest returned: {Path} - {StatusCode}.",
                        httpContext.Request.Path, httpContext.Response.StatusCode);
                }
                if (httpContext.Response.StatusCode == StatusCodes.Status408RequestTimeout) 
                {
                    _logger.LogWarning("Request timeout: {Path} - {StatusCode}.",
                        httpContext.Request.Path, httpContext.Response.StatusCode);
                }
            }
            catch (OperationCanceledException ex)
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                {
                    _logger.LogWarning(ex, "Request was cancelled due to timeout.");
                    await HandleTimeoutAsync(httpContext);
                }
                else
                {
                    _logger.LogWarning(ex, "Request was cancelled by client.");
                    await HandleClientDisconnectAsync(httpContext, ex);
                }
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Validation error.");
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsJsonAsync(new { error = ex.Message });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async static Task HandleClientDisconnectAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var response = new
            {
                error = "Client disconnected",
                message = exception.Message
            };
            await httpContext.Response.WriteAsJsonAsync(response);
        }
        private async static Task HandleTimeoutAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            var response = new
            {
                error = "Request Timeout",
                message = "The request took too long to process"
            };
            await httpContext.Response.WriteAsJsonAsync(response);
        }

        private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var response = new
            {
                error = "Internal Server Error",
                message = exception.Message
            };
            await httpContext.Response.WriteAsJsonAsync(response);
        }
    }
}

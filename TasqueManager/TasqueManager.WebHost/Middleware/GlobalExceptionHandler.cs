using MassTransit.Internals.GraphValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;

namespace TasqueManager.WebHost.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
        {
            await HandleExceptionAsync(httpContext, exception, cancellationToken);
            return true;
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server error",
                Detail = exception.Message
            };
            switch (exception) 
            {
                case ArgumentNullException:
                    _logger.LogWarning(exception, "Required parameter was not provided.");
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Invalid parameters.";
                    break;
                case KeyNotFoundException:
                    _logger.LogWarning(exception, "Requested resource not found.");
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Requested resource not found.";
                    break;
                case TimeoutException:
                    _logger.LogWarning(exception, "Request was cancelled due to timeout.");
                    problemDetails.Status = StatusCodes.Status408RequestTimeout;
                    problemDetails.Title = "Request timeout.";
                    break;
                case OperationCanceledException:
                    if (httpContext.RequestAborted.IsCancellationRequested)
                    {
                        _logger.LogWarning(exception, "Request was cancelled due to timeout.");
                        problemDetails.Status = StatusCodes.Status408RequestTimeout;
                        problemDetails.Title = "Request timeout.";
                    }
                    else
                    {
                        _logger.LogWarning(exception, "Request was cancelled by client.");
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "Client disconnected.";
                    }
                    break;
                case ValidationException:
                    _logger.LogWarning(exception, "Validation error.");
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Validation error.";
                    break;
                case HttpRequestException:
                    _logger.LogError(exception, "HTTP request failed.");
                    problemDetails.Status = StatusCodes.Status502BadGateway;
                    problemDetails.Title = "HTTP request failed.";
                    break;
                default:
                    _logger.LogError(exception, "Unhandled error occured.");
                    break;
            }
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);
        }
    }
}

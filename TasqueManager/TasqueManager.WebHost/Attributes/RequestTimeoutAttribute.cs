using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TasqueManager.WebHost.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestTimeoutAttribute : ActionFilterAttribute
    {
        private readonly TimeSpan _timeout;

        public RequestTimeoutAttribute(int seconds)
        {
            _timeout = TimeSpan.FromSeconds(seconds);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            using var cts = new CancellationTokenSource(_timeout);
            var originalToken = context.HttpContext.RequestAborted;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, originalToken);
            context.HttpContext.RequestAborted = linkedCts.Token;

            await next();
        }
    }
}

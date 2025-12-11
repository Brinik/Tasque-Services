using MassTransit;
using Message;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Contracts.Assignment;
using TasqueManager.Domain;
using TasqueManager.WebHost.Models;

namespace TasqueManager.WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrencyExchangeRateController : ControllerBase
    {
        private readonly ICurrencyExchangeRateService _service;

        public CurrencyExchangeRateController(ICurrencyExchangeRateService service,
            ILogger<AssignmentController> logger, IBusControl busControl)
        {
            _service = service;
        }

        [HttpGet("ExchangeRate")]
        [RequestTimeout("ShortTimeout")]
        public async Task<IActionResult> GetExchangeRateAsync()
        {
            var cancellationToken = HttpContext.RequestAborted;
            try
            {
                return Ok(await _service.GetExchangeRateAsync());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}

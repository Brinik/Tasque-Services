using MassTransit;
using Message;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using TasqueManager.Abstractions.ServiceAbstractions;

namespace TasqueManager.WebHost.Controllers
{
    public class DebugController : Controller
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IBusControl _busControl;

        public DebugController(ILogger<DebugController> logger, IBusControl busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        /// <summary>
        /// Послать сообщение rmq вручную
        /// </summary>
        /// <returns>guid сообщения</returns>
        [HttpGet("rmqMessage")]
        public async Task<IActionResult> SendMessage()
        {
            Guid guid = Guid.NewGuid();
            try
            {
                await _busControl.Publish(new MessageDto
                {
                    Content = $"Message with id {guid}"
                });
                _logger.LogInformation($"Message sent. Id: {guid}");
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(guid);
        }

        /// <summary>
        /// Проверка остановки по таймауту
        /// </summary>
        [HttpGet("slow-operation")]
        [RequestTimeout("ShortTimeout")]
        public async Task<IActionResult> SlowOperationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting slow operation at {Time}", DateTime.Now);
                await Task.Delay(10000, cancellationToken);

                _logger.LogInformation("Slow operation completed at {Time}", DateTime.Now);
                return Ok("Operation completed");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Проверка отлавливания исключений
        /// </summary>
        /// <returns>Ошибка 500</returns>
        [HttpGet("null-reference")]
        public IActionResult NullReference()
        {
            string test = null!;
            return Ok(test.Length);
        }
    }
}

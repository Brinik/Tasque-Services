using Message;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Consumers
{
    public class EventConsumer : IConsumer<MessageDto>
    {
        private readonly ILogger<EventConsumer> _logger;
        public EventConsumer(ILogger<EventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<MessageDto> context)
        {
            var messageId = context.MessageId?.ToString() ?? "unknown";
            var messageContent = context.Message.Content;

            using (_logger.BeginScope("MessageId: {MessageId}", messageId))
            {
                _logger.LogInformation(messageContent);
            }
        }
    }
}
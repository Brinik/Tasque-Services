using MassTransit;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Domain;
using TasqueManager.Contracts;

namespace TasqueManager.WebHost.Services
{
    public class DueDateCheckBS : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<DueDateCheckBS> _logger;
        private readonly IBusControl _busControl;
        public DueDateCheckBS(IServiceScopeFactory scopeFactory, IHostApplicationLifetime lifetime, ILogger<DueDateCheckBS> logger, IBusControl busControl) 
        {
            _scopeFactory = scopeFactory;
            _lifetime = lifetime;
            _logger = logger;
            _busControl = busControl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            if (!await WaitForAppStartup(_lifetime, stoppingToken))
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var assignmentRepository = scope.ServiceProvider.GetRequiredService<IAssignmentRepository>();
                        await CheckDueTimeAsync(assignmentRepository, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during due date check");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckDueTimeAsync(IAssignmentRepository assignmentRepository, CancellationToken stoppingToken) 
        {
            ICollection<Assignment> entities = await assignmentRepository.GetAllAsync(stoppingToken);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };
            await Parallel.ForEachAsync(entities, options, async (entity, stoppingToken) =>
            {
                if (entity != null && entity.DueDate.ToUniversalTime() < System.DateTime.UtcNow)
                {
                    entity.Status = AssignmentStatus.Overdue;
                    assignmentRepository.Update(entity);
                    await _busControl.Publish(new MessageDto
                    {
                        Content = $"Task {entity.Id} with title {entity.Title} is overdue"
                    }, stoppingToken);
                }
            });
            await assignmentRepository.SaveChangesAsync(stoppingToken);
        }
        static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
        {
            var startedSource = new TaskCompletionSource();
            using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());

            var cancelledSource = new TaskCompletionSource();
            using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

            Task completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

            return completedTask == startedSource.Task;
        }
    }
}

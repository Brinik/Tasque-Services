using BackgroundService.Consumers;
using BackgroundService.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Регистрируем EventConsumer в DI
                    services.AddScoped<EventConsumer>();
                    var applicationSettings = configuration.Get<ApplicationSettings>() ?? throw new NullReferenceException();
                    services.AddSingleton(applicationSettings);
                    services.AddMassTransit(x =>
                    {
                        // Добавляем consumer в MassTransit
                        x.AddConsumer<EventConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            try
                            {
                                ConfigureRmq(cfg, configuration);
                                RegisterEndPoints(cfg, context);
                                
                                var logger = context.GetRequiredService<ILogger<Program>>();
                                logger.LogInformation("MassTransit configured with RabbitMQ at: {Host}", 
                                    context.GetRequiredService<ApplicationSettings>().RmqSettings.Host);
                            }
                            catch (Exception ex)
                            {
                                var logger = context.GetRequiredService<ILogger<Program>>();
                                logger.LogError(ex, "Failed to configure RabbitMQ");
                                throw;
                            }
                        });

                    });

                    services.AddHostedService<MasstransitService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });
        }

        /// <summary>
        /// Конфигурирование RMQ.
        /// </summary>
        /// <param name="configurator"> Конфигуратор RMQ. </param>
        /// <param name="configuration"> Конфигурация приложения. </param>
        private static void ConfigureRmq(IRabbitMqBusFactoryConfigurator configurator, IConfiguration configuration)
        {
            var appSettings = configuration.Get<ApplicationSettings>() ?? throw new ArgumentNullException();
            var rmqSettings = appSettings.RmqSettings;

            configurator.Host(rmqSettings.Host,
                rmqSettings.VHost,
                h =>
                {
                    h.Username(rmqSettings.Login);
                    h.Password(rmqSettings.Password);
                });
        }

        /// <summary>
        /// регистрация эндпоинтов
        /// </summary>
        /// <param name="configurator">Конфигуратор RabbitMQ</param>
        /// <param name="context">Контекст регистрации (IServiceProvider)</param>
        private static void RegisterEndPoints(IRabbitMqBusFactoryConfigurator configurator, IBusRegistrationContext context)
        {
            configurator.ReceiveEndpoint($"masstransit_event_queue_1", e =>
            {
                e.ConfigureConsumer<EventConsumer>(context);
                e.UseMessageRetry(r =>
                {
                    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                });
                var logger = context.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Receive endpoint configured: masstransit_event_queue_1");
            });
        }
    }
}
using BackgroundService.Consumers;
using BackgroundService.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

                    services.AddMassTransit(x =>
                    {
                        // Добавляем consumer в MassTransit
                        x.AddConsumer<EventConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            ConfigureRmq(cfg, configuration);
                            RegisterEndPoints(cfg, context);
                        });
                    });

                    services.AddHostedService<MasstransitService>();
                });
        }

        /// <summary>
        /// Конфигурирование RMQ.
        /// </summary>
        /// <param name="configurator"> Конфигуратор RMQ. </param>
        /// <param name="configuration"> Конфигурация приложения. </param>
        private static void ConfigureRmq(IRabbitMqBusFactoryConfigurator configurator, IConfiguration configuration)
        {
            var rmqSettings = configuration.Get<ApplicationSettings>().RmqSettings;
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
                // Используем метод, который резолвит consumer из DI контейнера
                e.ConfigureConsumer<EventConsumer>(context);

                e.UseMessageRetry(r =>
                {
                    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                });
            });
        }
    }
}
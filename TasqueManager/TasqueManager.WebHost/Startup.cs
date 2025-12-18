using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using System.Text.Json.Serialization;
using TasqueManager.Abstractions.RepositoryAbstractions;
using TasqueManager.Abstractions.ServiceAbstractions;
using TasqueManager.Infrastructure;
using TasqueManager.Infrastructure.Repositories;
using TasqueManager.WebHost.Mapping;
using TasqueManager.WebHost.Middleware;
using TasqueManager.WebHost.Services;
using TasqueManager.WebHost.Settings;

namespace TasqueManager.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private IConfiguration _configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForeignUrlStrings>(_configuration.GetSection("ForeignUrlStrings"));
            services.AddMemoryCache();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            InstallAutomapper(services);
            AddServices(services, _configuration);
            services.AddControllers()
                    .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TasqueManager API",
                    Version = "v1"
                });
            });

            services.AddMassTransit(x => {
                x.UsingRabbitMq((context, cfg) =>
                {
                    ConfigureRmq(cfg, _configuration);
                });
            });
            services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter());
            services.AddRequestTimeouts(options =>
            {
                options.AddPolicy("ShortTimeout", TimeSpan.FromSeconds(5));
                options.AddPolicy("LongTimeout", TimeSpan.FromSeconds(60));
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler();
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TasqueManager API V1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseHsts();
            }
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseRequestTimeouts();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        private static IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
        {
            var applicationSettings = configuration.Get<ApplicationSettings>() ?? throw new NullReferenceException();

            services.AddSingleton(applicationSettings)
                        .AddDbContext<DatabaseContext>(optionsBuilder
            => optionsBuilder.UseNpgsql(applicationSettings.ConnectionString));

            services.AddSingleton((IConfigurationRoot)configuration)
                    .AddTransient<IAssignmentService, AssignmentService>()
                    .AddTransient<ICurrencyExchangeRateService, CurrencyExchangeService>()
                    .AddScoped<IAssignmentRepository, AssignmentRepository>()
                    .AddScoped<IUnitOfWork, UnitOfWork>()
                    .AddHostedService<DueDateCheckBS>();
            return services;
        }
        private static IServiceCollection InstallAutomapper(IServiceCollection services)
        {
            services.AddSingleton<IMapper>(new Mapper(GetMapperConfiguration()));
            return services;
        }
        private static MapperConfiguration GetMapperConfiguration()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AssignmentMappingProfile>();
            }, new NullLoggerFactory());
            configuration.AssertConfigurationIsValid();
            return configuration;
        }

        /// <summary>
        /// Конфигурирование RMQ.
        /// </summary>
        /// <param name="configurator"> Конфигуратор RMQ. </param>
        /// <param name="configuration"> Конфигурация приложения. </param>
        private static void ConfigureRmq(IRabbitMqBusFactoryConfigurator configurator, IConfiguration configuration)
        {
            var applicationSettings = configuration.Get<ApplicationSettings>() ??
                throw new NullReferenceException();

            var rmqSettings = applicationSettings.RmqSettings ?? throw new NullReferenceException();
            configurator.Host(rmqSettings.Host,
                rmqSettings.VHost,
                h =>
                {
                    h.Username(rmqSettings.Login);
                    h.Password(rmqSettings.Password);
                });
        }
    }
}


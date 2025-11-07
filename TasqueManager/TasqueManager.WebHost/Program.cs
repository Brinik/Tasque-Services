using Microsoft.AspNetCore.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using TasqueManager.Domain;
using TasqueManager.Infrastructure;
using TasqueManager.WebHost;
using Microsoft.EntityFrameworkCore;


namespace WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                db.Database.EnsureDeletedAsync();
                db.Database.Migrate();
                Seed(scope.ServiceProvider);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) => { });
                });

        public static void Seed(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DatabaseContext>() ?? throw new NullReferenceException();
                var assignment = new Assignment()
                {
                    Id = new Guid("329CA2C8-F106-4E5C-BE6A-EA7AF90F6627"),
                    Title = "Title",
                    Description = "Description",
                    DueDate = new DateTime(),
                    Status = AssignmentStatus.New,
                    Deleted = false
                };
                context.Assignments.Add(assignment);
                context.SaveChanges();
            }
        }
    }
}

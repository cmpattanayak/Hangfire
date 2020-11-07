using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Hangfire.Server
{
    public static class Program
    {
        private static BackgroundJobServer backgroundJobServer { get; set; }
        private static IConfiguration configuration = null;

        static void Main(string[] args)
        {
            Program.configuration = LoadConfiguration();

            // Create service collection and configure our services
            var services = ConfigureServices();
            //Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            StartHangfireServer(serviceProvider);
            RegisterJob();

            System.Console.ReadKey();
        }

        public static void StartHangfireServer(IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetSection("HangfireConnection").Value, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                })
                .UseDefaultTypeResolver()
                .UseDefaultTypeSerializer()
                .UseActivator(new HFJobActivator(serviceProvider))
                .UseConsole();

            BackgroundJobServerOptions backgroundJobServerOptions = new BackgroundJobServerOptions
            {
                WorkerCount = Environment.ProcessorCount * 5,
                ServerName = "Hangfire Job Server",
            };

            backgroundJobServer = new BackgroundJobServer(backgroundJobServerOptions);
        }

        private static void RegisterJob()
        {
            RecurringJob.AddOrUpdate<IJobProcessor>(x => x.PrintMessage(null), "* * * * *");
        }

        private static void GracefulServerShutdown(object sender, EventArgs e)
        {
            backgroundJobServer.WaitForShutdown(new TimeSpan(0, 0, 0));        //Shutdown immediately
            backgroundJobServer.Dispose();
        }

        private static IServiceCollection ConfigureServices()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(GracefulServerShutdown);
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddLogging();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IJobProcessor, JobProcessor>();

            return services;
        }

        private static IConfiguration LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddEnvironmentVariables()
                                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return configurationBuilder.Build();
        }
    }
}

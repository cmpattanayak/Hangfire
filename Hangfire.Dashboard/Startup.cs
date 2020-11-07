using System;
using System.IO;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hangfire.Dashboard
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            this.Configuration = LoadConfiguration();
            string conStr = this.Configuration.GetSection("HangfireConnection").Value;

            services.AddTransient<IJobProcessor, JobProcessor>();

            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(conStr)
                    .UseConsole(new ConsoleOptions { BackgroundColor = "#008080" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            DashboardOptions dashboardOptions = new DashboardOptions
            {
                DisplayStorageConnectionString = false,
                AppPath = null,
                DashboardTitle = "Hangfire Scheduler - " + AppEnvironment.ToUpper(),
                IsReadOnlyFunc = (DashboardContext context) => false,
            };

            app.UseHangfireDashboard("/hangfire", dashboardOptions);
        }

        private static string AppEnvironment
        {
            get
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
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

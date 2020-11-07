# Scheduling Jobs with ASP.Net Core and Hangfire
## Overview
Hangfire provides an easy way to perform background processing in .NET and .NET Core applications. No Windows Service or separate process required.
It is backed by persistent storage. Background jobs are created in a persistent storage – SQL Server, Redis, PostgreSQL, MongoDB etc. 
You can safely restart your application and use Hangfire with ASP.NET without worrying about application pool recycles.
Hangfire is open and free for commercial use.
## Job Types
  - Fire and forget job (are executed only once and almost immediately after creation, can be triggered on certain events)
  - Delayed jobs (are executed only once too, but not immediately, after a certain time interval) 
  - Recurring jobs (recurring jobs fire many times on the specified CRON schedule)
  - Continuations (are executed when its parent job has been finished)
## What I have used in this project
1. Asp.Net Core 3.1
2. Hangfire 1.7.x (Following Nuget Packages)
   - Hangfire.AspNetCore
   - Hangfire.SqlServer
   - Hangfire.Console
3. I have used Sql Server for Storage
## Scheduler Components
 1. **Server** - 
 It is the most important component of Hangfire Schedulers. It has to run continuously, so when time comes, it will execute specific job(s).
 A console application can fulfill this requirement.
 2. **Client (Optional)** - 
 If you have to configure a fire and forget job which need to be triggered on some events (say a button click), you may need a clicent application which can be built on anything.
 3. **Dashboard (Optional)** - 
 If you want to monitor job related activities like how many jobs have succeeded/failed or in process or if you want to log stepwise execution progress, you may require a dashboard.
 This has to be a web application (in this case Asp.Net Core Web Application).
### What is common in these three components 
 You might be thinking how these three apps are interlinked to each other. Well, the answer is the Sql Server connection. 
 You need to provide the same Sql Server connection string in each of these applications.
## Server Configuration
 Use following simple configurations during start up (Main method).
 ```
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
```
"HangfireConnection" is your Sql Server connection string.
> Note - Before running your application, Create the Database in Sql Server, rest of the things will be taken care by Hangfire.

Register your jobs after configuration is done.
```
RecurringJob.AddOrUpdate<IJobProcessor>(x => x.PrintMessage(null), "* * * * *");
```
The asterisk pattern is CRON expression which in this case indicates the job would run every minute.
## Dashboard Configuration
Ready your dashboard application with following code in ConfigureServices method of Startup class.
```
services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(conStr)
        .UseConsole(new ConsoleOptions { BackgroundColor = "#008080" }));
```
"conStr" is the same Sql Server connection string which you used in Server.

Customize your dashboard with following dasboard configurations in Configure method.
```
DashboardOptions dashboardOptions = new DashboardOptions
            {
                DisplayStorageConnectionString = false,
                AppPath = null,
                DashboardTitle = "Hangfire Scheduler - " + AppEnvironment.ToUpper(),
                IsReadOnlyFunc = (DashboardContext context) => false
            };

            app.UseHangfireDashboard("/hangfire", dashboardOptions);
```

"/hangfire" denotes you can acces your dashboard with [{Root Url}/hangfire]

By default, all buttons are read only in hangfire dashboard. If you wish to make those enable, use following code:
```
IsReadOnlyFunc = (DashboardContext context) => false,
```
If you navigate to Dashboard Jobs Screen - the page will look like following.

![dashboard](https://github.com/cmpattanayak/Hangfire/blob/master/hangfire_dashboard_01.JPG)

Click a Job ID to see the Progress/Status/Logs.

![dashboard](https://github.com/cmpattanayak/Hangfire/blob/master/hangfire_dashboard_02.JPG)

## Reference
https://www.hangfire.io/

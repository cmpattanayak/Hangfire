using Hangfire.Console;
using Hangfire.Console.Progress;
using System;

namespace Hangfire.Server
{
    public class JobProcessor : IJobProcessor
    {
        public void PrintMessage(PerformContext context)
        {
            IProgressBar progress = context.WriteProgressBar("Processing", 0, ConsoleTextColor.DarkBlue);

            progress.SetValue(50);
            context.WriteLine(ConsoleTextColor.White, $"Job Started at {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt")}");

            //Do the actual task here
            System.Console.WriteLine("Hangfire Says Hello!");

            context.WriteLine(ConsoleTextColor.White, $"Job Completed at {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt")}");
            progress.SetValue(100);
        }
    }
}

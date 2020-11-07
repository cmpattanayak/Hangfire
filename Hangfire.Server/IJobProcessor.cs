namespace Hangfire.Server
{
    public interface IJobProcessor
    {
        [AutomaticRetry(Attempts = 0)]
        [JobDisplayName("Print Message - Job")]
        [JobRetentionDays(7)]
        void PrintMessage(PerformContext context);
    }
}

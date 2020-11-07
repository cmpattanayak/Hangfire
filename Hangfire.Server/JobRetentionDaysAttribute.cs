using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;

namespace Hangfire.Server
{
    public class JobRetentionDaysAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private int days { get; set; }
        public JobRetentionDaysAttribute(int Days) => this.days = Days;
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => context.JobExpirationTimeout = TimeSpan.FromDays(this.days);
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction) => context.JobExpirationTimeout = TimeSpan.FromDays(this.days);
    }
}

using System;

namespace Hangfire.Server
{
    public class HFJobActivator : JobActivator
    {
        private readonly IServiceProvider _container;
        public HFJobActivator(IServiceProvider container) => this._container = container;
        public override object ActivateJob(Type jobType) => this._container.GetService(jobType);
    }
}

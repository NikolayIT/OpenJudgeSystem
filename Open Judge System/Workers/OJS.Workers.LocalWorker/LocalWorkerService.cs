namespace OJS.Workers.LocalWorker
{
    using log4net;

    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers.Jobs;

    internal class LocalWorkerService : LocalWorkerServiceBase<int>
    {
        private readonly ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness;

        public LocalWorkerService(ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness) =>
            this.submissionsForProcessingBusiness = submissionsForProcessingBusiness;

        protected override void BeforeStartingThreads(ILog logger) =>
            this.submissionsForProcessingBusiness.ResetAllProcessingSubmissions(logger);

        protected override IDependencyContainer GetDependencyContainer() => Bootstrap.Container;
    }
}
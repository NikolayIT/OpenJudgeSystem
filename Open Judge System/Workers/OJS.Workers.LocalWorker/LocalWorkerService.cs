namespace OJS.Workers.LocalWorker
{
    using System;

    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers.Jobs;

    internal class LocalWorkerService : LocalWorkerServiceBase<int>
    {
        private readonly ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness;

        public LocalWorkerService(ISubmissionsForProcessingBusinessService submissionsForProcessingBusiness) =>
            this.submissionsForProcessingBusiness = submissionsForProcessingBusiness;

        protected override void BeforeStartingThreads()
        {
            try
            {
                this.submissionsForProcessingBusiness.ResetAllProcessingSubmissions();
            }
            catch (Exception ex)
            {
                this.Logger.FatalFormat($"Resetting Processing submissions failed with exception {ex.Message}");
                throw;
            }

            base.BeforeStartingThreads();
        }

        protected override IDependencyContainer GetDependencyContainer() => Bootstrap.Container;
    }
}
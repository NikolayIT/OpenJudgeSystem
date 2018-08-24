namespace OJS.Workers.LocalWorker
{
    using System;

    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers.Jobs;

    internal class LocalWorkerService : LocalWorkerServiceBase<int>
    {
        protected override void BeforeStartingThreads()
        {
            try
            {
                using (this.DependencyContainer.BeginDefaultScope())
                {
                    var submissionsForProcessingBusiness =
                        this.DependencyContainer.GetInstance<ISubmissionsForProcessingBusinessService>();

                    submissionsForProcessingBusiness.ResetAllProcessingSubmissions();
                }
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
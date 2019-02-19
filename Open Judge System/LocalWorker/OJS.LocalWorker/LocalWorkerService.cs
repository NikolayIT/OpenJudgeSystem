namespace OJS.LocalWorker
{
    using System;

    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Workers;
    using OJS.Workers.Common;

    internal class LocalWorkerService : LocalWorkerServiceBase<int>
    {
        protected override IDependencyContainer GetDependencyContainer() => Bootstrap.Container;

        protected override void BeforeStartingThreads()
        {
            try
            {
                EncryptionHelper.EncryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                this.Logger.Warn("Cannot encrypt App.config", ex);
            }

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

            try
            {
                this.StartMonitoringService();
            }
            catch (Exception ex)
            {
                this.Logger.Error(
                    $"An exception was thrown while attempting to start the {Constants.LocalWorkerMonitoringServiceName}",
                    ex);
            }

            base.BeforeStartingThreads();
        }

        protected override void BeforeAbortingThreads()
        {
            try
            {
                EncryptionHelper.DecryptAppConfigSections(Constants.AppSettingsConfigSectionName);
            }
            catch (Exception ex)
            {
                this.Logger.Warn("Cannot decrypt App.config", ex);
            }

            base.BeforeAbortingThreads();
        }

        private void StartMonitoringService()
        {
            const string monitoringServiceName = Constants.LocalWorkerMonitoringServiceName;

            var serviceState = ServicesHelper.GetServiceState(monitoringServiceName);
            if (serviceState.Equals(ServiceState.Running))
            {
                this.Logger.Info($"{monitoringServiceName} is running.");
                return;
            }

            this.Logger.Info($"Attempting to start the {monitoringServiceName}...");

            if (serviceState.Equals(ServiceState.NotFound))
            {
                ServicesHelper.InstallService(monitoringServiceName, Settings.MonitoringServiceExecutablePath);
            }

            ServicesHelper.StartService(monitoringServiceName);

            this.Logger.Info($"{monitoringServiceName} started successfully.");
        }
    }
}
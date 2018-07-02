namespace OJS.Workers.Common.Helpers
{
    using System;

    using log4net;

    using OJS.Common.Helpers;

    public static class LocalWorkerServicesHelper
    {
        public static void TryStartService(string serviceName, ILog logger)
        {
            try
            {
                logger.Info($"Attempting to start the {serviceName}...");

                ServicesHelper.StartService(serviceName);

                logger.Info($"{serviceName} started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while attempting to start the {serviceName}", ex);
            }
        }

        public static void TryInstallAndStartService(string serviceName, string serviceExePath, ILog logger)
        {
            try
            {
                logger.Info($"Attempting to install and start the {serviceName}...");

                ServicesHelper.InstallAndStart(serviceName, serviceExePath);

                logger.Info($"{serviceName} installed and started successfully.");
            }
            catch (Exception ex)
            {
                logger.Error($"An exception was thrown while attempting to install and start the {serviceName}", ex);
            }
        }
    }
}

namespace OJS.Workers.Common.ServiceInstaller
{
    using System.Runtime.InteropServices;

    using OJS.Workers.Common.ServiceInstaller.Models;

    [StructLayout(LayoutKind.Sequential)]
    internal class ServiceStatus
    {
        public int dwServiceType = 0;
        public ServiceState dwCurrentState = 0;
        public int dwControlsAccepted = 0;
        public int dwWin32ExitCode = 0;
        public int dwServiceSpecificExitCode = 0;
        public int dwCheckPoint = 0;
        public int dwWaitHint = 0;
    }
}

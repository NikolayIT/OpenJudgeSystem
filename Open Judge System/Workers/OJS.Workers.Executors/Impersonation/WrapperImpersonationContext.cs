namespace OJS.Workers.Executors.Impersonation
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;

    using OJS.Workers.Executors.Process;

    public class WrapperImpersonationContext
    {
        private const int Logon32ProviderDefault = 0;
        private const int Logon32LogonInteractive = 2;

        private readonly string domain;
        private readonly string password;
        private readonly string username;
        private IntPtr token;

        private WindowsImpersonationContext context;

        public WrapperImpersonationContext(string domain, string username, string password)
        {
            this.domain = domain;
            this.username = username;
            this.password = password;
        }

        protected bool IsInContext
        {
            get { return this.context != null; }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Enter()
        {
            if (this.IsInContext)
            {
                return;
            }

            this.token = new IntPtr(0);
            try
            {
                this.token = IntPtr.Zero;
                bool logonSuccessfull = NativeMethods.LogonUser(
                   this.username, 
                   this.domain, 
                   this.password, 
                   Logon32LogonInteractive, 
                   Logon32ProviderDefault, 
                   ref this.token);
                if (logonSuccessfull == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }

                var identity = new WindowsIdentity(this.token);
                this.context = identity.Impersonate();
            }
            catch
            {
                // Catch exceptions here
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Leave()
        {
            if (this.IsInContext == false)
            {
                return;
            }

            this.context.Undo();

            if (this.token != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(this.token);
            }

            this.context = null;
        }
    }
}

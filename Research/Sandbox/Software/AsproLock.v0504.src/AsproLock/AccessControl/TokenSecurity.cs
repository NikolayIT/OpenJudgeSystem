using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class TokenSecurity : BaseSecurity
    {

        #region Constructors

        public TokenSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public TokenSecurity(Process process, AccessControlSections sectionsRequested)
            : this(process.Id, sectionsRequested, true)
        {
        }

        public TokenSecurity(ProcessThread thread, AccessControlSections sectionsRequested)
            : this(thread.Id, sectionsRequested, false)
        {
        }

        public TokenSecurity(int id, AccessControlSections sectionsRequested, bool idIsProcessID)
            : base(GetReadHandle(id, idIsProcessID), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public TokenSecurity(IntPtr tokenHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(tokenHandle), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        private static GenericSafeHandle GetReadHandle(int id, bool idIsProcessID)
        {
            IntPtr tempHandle = IntPtr.Zero;
            IntPtr readHandle = IntPtr.Zero;

            if (idIsProcessID)
            {
                GenericSafeHandle procHandle = new GenericSafeHandle(NativeMethods.OpenProcess((int)ProcessRights.QueryInformation, false, id));
                if (procHandle.IsInvalid)
                {
                    int err = Marshal.GetLastWin32Error();
                    switch (err)
                    {
                        case NativeConstants.ERROR_ACCESS_DENIED:
                            throw new UnauthorizedAccessException();
                        default:
                            throw new System.ComponentModel.Win32Exception(err);
                    }
                }
                using (procHandle)
                {
                    if (!NativeMethods.OpenProcessToken(procHandle, (int)TokenRights.ReadPermissions, out readHandle))
                    {
                        int err = Marshal.GetLastWin32Error();
                        switch (err)
                        {
                            case NativeConstants.ERROR_ACCESS_DENIED:
                                throw new UnauthorizedAccessException();
                            default:
                                throw new System.ComponentModel.Win32Exception(err);
                        }
                    }
                }
            }
            else
            {
                GenericSafeHandle threadHandle = new GenericSafeHandle(NativeMethods.OpenThread((int)ThreadRights.QueryInformation, false, id));
                if (threadHandle.IsInvalid)
                {
                    int err = Marshal.GetLastWin32Error();
                    switch (err)
                    {
                        case NativeConstants.ERROR_ACCESS_DENIED:
                            throw new UnauthorizedAccessException();
                        default:
                            throw new System.ComponentModel.Win32Exception(err);
                    }
                }
                using (threadHandle)
                {
                    if (!NativeMethods.OpenThreadToken(threadHandle, (int)TokenRights.ReadPermissions, false, out readHandle))
                    {
                        int err = Marshal.GetLastWin32Error();
                        switch (err)
                        {
                            case NativeConstants.ERROR_ACCESS_DENIED:
                                throw new UnauthorizedAccessException();
                            default:
                                throw new System.ComponentModel.Win32Exception(err);
                        }
                    }
                }
            }

            return new GenericSafeHandle(readHandle);
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(TokenRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new TokenAccessRule(identityReference, (TokenRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(TokenAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new TokenAuditRule(identityReference, (TokenRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(TokenAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Token specific types.

        public void AddAccessRule(TokenAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(TokenAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(TokenAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(TokenAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(TokenAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(TokenAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(TokenAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(TokenAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(TokenAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(TokenAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(TokenAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)TokenRights.Read, (int)TokenRights.Write,
                                 (int)TokenRights.Execute, (int)TokenRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

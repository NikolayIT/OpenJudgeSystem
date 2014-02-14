using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class ProcessSecurity : BaseSecurity
    {

        #region Constructors

        public ProcessSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public ProcessSecurity(int processID, AccessControlSections sectionsRequested)
            : base(GetReadHandle(processID), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public ProcessSecurity(Process process, AccessControlSections sectionsRequested)
            : this(process.Id, sectionsRequested)
        {
        }

        public ProcessSecurity(IntPtr processHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(processHandle), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        #endregion

        private static GenericSafeHandle GetReadHandle(int processId)
        {
            IntPtr readHandle = NativeMethods.OpenProcess((int)ProcessRights.ReadPermissions, false, processId);
            if (readHandle == IntPtr.Zero)
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
            return new GenericSafeHandle(readHandle);
        }

        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(ProcessRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new ProcessAccessRule(identityReference, (ProcessRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(ProcessAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new ProcessAuditRule(identityReference, (ProcessRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(ProcessAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Process specific types.

        public void AddAccessRule(ProcessAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(ProcessAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(ProcessAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(ProcessAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(ProcessAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(ProcessAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(ProcessAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(ProcessAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(ProcessAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(ProcessAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(ProcessAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)ProcessRights.Read, (int)ProcessRights.Write,
                                 (int)ProcessRights.Execute, (int)ProcessRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

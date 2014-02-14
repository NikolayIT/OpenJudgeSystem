using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class JobObjectSecurity : BaseSecurity
    {

        #region Constructors

        public JobObjectSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public JobObjectSecurity(string jobName, AccessControlSections sectionsRequested)
            : base(GetReadHandle(jobName), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public JobObjectSecurity(IntPtr jobHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(jobHandle), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        private static GenericSafeHandle GetReadHandle(string jobName)
        {
            IntPtr readHandle = NativeMethods.OpenJobObject((int)JobObjectRights.ReadPermissions, false, jobName);
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

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(JobObjectRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new JobObjectAccessRule(identityReference, (JobObjectRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(JobObjectAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new JobObjectAuditRule(identityReference, (JobObjectRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(JobObjectAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for JobObject specific types.

        public void AddAccessRule(JobObjectAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(JobObjectAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(JobObjectAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(JobObjectAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(JobObjectAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(JobObjectAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(JobObjectAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(JobObjectAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(JobObjectAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(JobObjectAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(JobObjectAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)JobObjectRights.Read, (int)JobObjectRights.Write,
                                 (int)JobObjectRights.Execute, (int)JobObjectRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

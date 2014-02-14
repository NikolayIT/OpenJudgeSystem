using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class ThreadSecurity : BaseSecurity
    {

        #region Constructors

        public ThreadSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public ThreadSecurity(int threadID, AccessControlSections sectionsRequested)
            : base(GetReadHandle(threadID), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public ThreadSecurity(ProcessThread thread, AccessControlSections sectionsRequired)
            : this(thread.Id, sectionsRequired)
        {
        }

        public ThreadSecurity(IntPtr threadHandle, AccessControlSections sectionsRequired)
            : base(BaseSecurity.GetReadHandle(threadHandle), ResourceType.KernelObject, sectionsRequired, false)
        {
        }

        #endregion

        private static GenericSafeHandle GetReadHandle(int threadId)
        {
            IntPtr readHandle = NativeMethods.OpenThread((int)ThreadRights.ReadPermissions, false, threadId);
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
            get { return typeof(ThreadRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new ThreadAccessRule(identityReference, (ThreadRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(ThreadAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new ThreadAuditRule(identityReference, (ThreadRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(ThreadAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Thread specific types.

        public void AddAccessRule(ThreadAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(ThreadAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(ThreadAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(ThreadAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(ThreadAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(ThreadAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(ThreadAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(ThreadAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(ThreadAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(ThreadAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(ThreadAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)ThreadRights.Read, (int)ThreadRights.Write,
                                 (int)ThreadRights.Execute, (int)ThreadRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

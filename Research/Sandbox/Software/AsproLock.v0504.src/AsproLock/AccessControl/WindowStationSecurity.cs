using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class WindowStationSecurity : BaseSecurity
    {

        #region Constructors

        public WindowStationSecurity()
            : base(ResourceType.WindowObject, false)
        {
        }

        public WindowStationSecurity(string windowStationName, AccessControlSections sectionsRequested)
            : base(GetReadHandle(windowStationName), ResourceType.WindowObject, sectionsRequested, false)
        {
        }

        public WindowStationSecurity(IntPtr windowStationHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(windowStationHandle, NativeMethods.CloseWindowStation), ResourceType.WindowObject, sectionsRequested, false)
        {
        }

        private static GenericSafeHandle GetReadHandle(string windowStationName)
        {
            IntPtr readHandle = NativeMethods.OpenWindowStation(windowStationName, false, (int)StandardRights.ReadPermissions);
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
            return new GenericSafeHandle(readHandle, NativeMethods.CloseWindowStation);
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(WindowStationRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new WindowStationAccessRule(identityReference, (WindowStationRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(WindowStationAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new WindowStationAuditRule(identityReference, (WindowStationRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(WindowStationAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for WindowStation specific types.

        public void AddAccessRule(WindowStationAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(WindowStationAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(WindowStationAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(WindowStationAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(WindowStationAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(WindowStationAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(WindowStationAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(WindowStationAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(WindowStationAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(WindowStationAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(WindowStationAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)WindowStationRights.Read, (int)WindowStationRights.Write,
                                 (int)WindowStationRights.Execute, (int)WindowStationRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

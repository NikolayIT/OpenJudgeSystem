using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class DirectorySecurity : BaseSecurity
    {

        #region Constructors

        public DirectorySecurity()
            : base(ResourceType.FileObject, true)
        {
        }

        public DirectorySecurity(string directoryName, AccessControlSections sectionsRequested)
            : base(directoryName, ResourceType.FileObject, sectionsRequested, true)
        {
        }

        public DirectorySecurity(IntPtr directoryHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(directoryHandle), ResourceType.FileObject, sectionsRequested, true)
        {
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(DirectoryRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new DirectoryAccessRule(identityReference, (DirectoryRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(DirectoryAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new DirectoryAuditRule(identityReference, (DirectoryRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(DirectoryAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Directory specific types.

        public void AddAccessRule(DirectoryAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(DirectoryAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(DirectoryAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(DirectoryAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(DirectoryAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(DirectoryAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(DirectoryAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(DirectoryAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(DirectoryAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(DirectoryAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(DirectoryAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)DirectoryRights.Read, (int)DirectoryRights.Write,
                                 (int)DirectoryRights.Execute, (int)DirectoryRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion
    }
}

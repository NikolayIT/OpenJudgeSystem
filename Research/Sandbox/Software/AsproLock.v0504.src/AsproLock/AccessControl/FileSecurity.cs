using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class FileSecurity : BaseSecurity
    {

        #region Constructors

        public FileSecurity()
            : base(ResourceType.FileObject, false)
        {
        }

        public FileSecurity(string fileName, AccessControlSections sectionsRequested)
            : base(fileName, ResourceType.FileObject, sectionsRequested, false)
        {
        }

        public FileSecurity(IntPtr fileHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(fileHandle), ResourceType.FileObject, sectionsRequested, false)
        {
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(FileRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new FileAccessRule(identityReference, (FileRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(FileAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new FileAuditRule(identityReference, (FileRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(FileAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for File specific types.

        public void AddAccessRule(FileAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(FileAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(FileAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(FileAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(FileAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(FileAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(FileAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(FileAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(FileAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(FileAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(FileAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)FileRights.Read, (int)FileRights.Write,
                                 (int)FileRights.Execute, (int)FileRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

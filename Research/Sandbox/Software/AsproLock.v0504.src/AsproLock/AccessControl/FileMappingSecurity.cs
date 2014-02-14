using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class FileMappingSecurity : BaseSecurity
    {

        #region Constructors

        public FileMappingSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public FileMappingSecurity(string fileMappingName, AccessControlSections sectionsRequested)
            : base(fileMappingName, ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public FileMappingSecurity(IntPtr fileMappingHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(fileMappingHandle), ResourceType.KernelObject, sectionsRequested, false)
        {
        }


        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(FileMappingRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new FileMappingAccessRule(identityReference, (FileMappingRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(FileMappingAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new FileMappingAuditRule(identityReference, (FileMappingRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(FileMappingAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for FileMapping specific types.

        public void AddAccessRule(FileMappingAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(FileMappingAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(FileMappingAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(FileMappingAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(FileMappingAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(FileMappingAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(FileMappingAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(FileMappingAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(FileMappingAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(FileMappingAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(FileMappingAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)FileMappingRights.Read, (int)FileMappingRights.Write,
                                 (int)FileMappingRights.Execute, (int)FileMappingRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

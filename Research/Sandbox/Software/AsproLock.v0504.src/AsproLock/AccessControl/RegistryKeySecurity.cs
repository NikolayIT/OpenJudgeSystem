using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class RegistryKeySecurity : BaseSecurity
    {

        #region Constructors

        public RegistryKeySecurity()
            : base(ResourceType.RegistryKey, true)
        {
        }

        public RegistryKeySecurity(string registryKeyName, AccessControlSections sectionsRequested)
            : base(registryKeyName, ResourceType.RegistryKey, sectionsRequested, true)
        {
        }

        public RegistryKeySecurity(RegistryKey key, AccessControlSections sectionsRequested)
            : base(key.Name, ResourceType.RegistryKey, sectionsRequested, true)
        {
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(RegistryKeyRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new RegistryKeyAccessRule(identityReference, (RegistryKeyRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(RegistryKeyAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new RegistryKeyAuditRule(identityReference, (RegistryKeyRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(RegistryKeyAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for RegistryKey specific types.

        public void AddAccessRule(RegistryKeyAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(RegistryKeyAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(RegistryKeyAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(RegistryKeyAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(RegistryKeyAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(RegistryKeyAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(RegistryKeyAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(RegistryKeyAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(RegistryKeyAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(RegistryKeyAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(RegistryKeyAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)RegistryKeyRights.Read, (int)RegistryKeyRights.Write,
                                 (int)RegistryKeyRights.Execute, (int)RegistryKeyRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

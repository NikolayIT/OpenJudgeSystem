using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class WaitObjectSecurity : BaseSecurity
    {

        #region Constructors

        public WaitObjectSecurity()
            : base(ResourceType.KernelObject, false)
        {
        }

        public WaitObjectSecurity(string waitObjectName, AccessControlSections sectionsRequested)
            : base(waitObjectName, ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        public WaitObjectSecurity(IntPtr waitObjectHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(waitObjectHandle), ResourceType.KernelObject, sectionsRequested, false)
        {
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(WaitObjectRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new WaitObjectAccessRule(identityReference, (WaitObjectRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(WaitObjectAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new WaitObjectAuditRule(identityReference, (WaitObjectRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(WaitObjectAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for WaitObject specific types.

        public void AddAccessRule(WaitObjectAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(WaitObjectAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(WaitObjectAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(WaitObjectAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(WaitObjectAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(WaitObjectAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(WaitObjectAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(WaitObjectAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(WaitObjectAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(WaitObjectAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(WaitObjectAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)WaitObjectRights.Read, (int)WaitObjectRights.Write,
                                 (int)WaitObjectRights.Execute, (int)WaitObjectRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

using System;
using System.Security.AccessControl;

namespace Asprosys.Security.AccessControl
{
    [Serializable]
    public class ShareSecurity : BaseSecurity
    {

        #region Constructors

        public ShareSecurity()
            : base(ResourceType.LMShare, false)
        {
        }

        public ShareSecurity(string shareName, AccessControlSections sectionsRequested)
            : base(shareName, ResourceType.LMShare, sectionsRequested, false)
        {
        }


        #endregion

        #region NativeSecurityObject Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(ShareRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new ShareAccessRule(identityReference, (ShareRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(ShareAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new ShareAuditRule(identityReference, (ShareRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(ShareAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Share specific types.

        public void AddAccessRule(ShareAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(ShareAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(ShareAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(ShareAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(ShareAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(ShareAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(ShareAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(ShareAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(ShareAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(ShareAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(ShareAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)ShareRights.Read, (int)ShareRights.Write,
                                 (int)ShareRights.Execute, (int)ShareRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

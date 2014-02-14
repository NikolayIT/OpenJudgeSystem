using System;
using System.Security.AccessControl;

namespace Asprosys.Security.AccessControl
{
    [Serializable]
    public class ServiceSecurity : BaseSecurity
    {

        #region Constructors

        public ServiceSecurity()
            : base(ResourceType.Service, false)
        {
        }

        public ServiceSecurity(string serviceName, AccessControlSections sectionsRequested)
            : base(serviceName, ResourceType.Service, sectionsRequested, false)
        {
        }


        #endregion

        #region NativeSecurityObject Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(ServiceRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new ServiceAccessRule(identityReference, (ServiceRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(ServiceAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new ServiceAuditRule(identityReference, (ServiceRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(ServiceAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Service specific types.

        public void AddAccessRule(ServiceAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(ServiceAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(ServiceAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(ServiceAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(ServiceAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(ServiceAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(ServiceAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(ServiceAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(ServiceAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(ServiceAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(ServiceAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)ServiceRights.Read, (int)ServiceRights.Write,
                                 (int)ServiceRights.Execute, (int)ServiceRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

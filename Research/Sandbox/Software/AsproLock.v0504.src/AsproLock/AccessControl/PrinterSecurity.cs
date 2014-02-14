using System;
using System.Security.AccessControl;

namespace Asprosys.Security.AccessControl
{
    [Serializable]
    public class PrinterSecurity : BaseSecurity
    {

        #region Constructors

        public PrinterSecurity()
            : base(ResourceType.Printer, false)
        {
        }

        public PrinterSecurity(string printerName, AccessControlSections sectionsRequested)
            : base(printerName, ResourceType.Printer, sectionsRequested, false)
        {
        }


        #endregion

        #region NativeSecurityObject Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(PrinterRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new PrinterAccessRule(identityReference, (PrinterRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(PrinterAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new PrinterAuditRule(identityReference, (PrinterRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(PrinterAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Service specific types.

        public void AddAccessRule(PrinterAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(PrinterAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(PrinterAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(PrinterAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(PrinterAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(PrinterAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(PrinterAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(PrinterAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(PrinterAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(PrinterAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(PrinterAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)PrinterRights.Read, (int)PrinterRights.Write,
                                 (int)PrinterRights.Execute, (int)PrinterRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

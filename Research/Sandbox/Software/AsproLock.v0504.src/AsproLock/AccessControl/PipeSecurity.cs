using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    public sealed class PipeSecurity : BaseSecurity
    {

        #region Constructors

        public PipeSecurity()
            : base(ResourceType.FileObject, false)
        {
        }

        public PipeSecurity(string pipeName, AccessControlSections sectionsRequested)
            : base("\\\\.\\pipe\\" + pipeName, ResourceType.FileObject, sectionsRequested, false)
        {
        }

        public PipeSecurity(IntPtr pipeHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(pipeHandle), ResourceType.FileObject, sectionsRequested, false)
        {
        }

        //Probably not needed, we'll see.
        //private static GenericSafeHandle GetReadHandle(string pipeName)
        //{
        //    IntPtr readHandle = NativeMethods.OpenPipeHandle(pipeName, (int)PipeRights.ReadPermissions);
        //    return new GenericSafeHandle(readHandle);
        //}

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        public override Type AccessRightType
        {
            get { return typeof(PipeRights); }
        }

        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new PipeAccessRule(identityReference, (PipeRights)accessMask, type);
        }

        public override Type AccessRuleType
        {
            get { return typeof(PipeAccessRule); }
        }

        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new PipeAuditRule(identityReference, (PipeRights)accessMask, flags);
        }

        public override Type AuditRuleType
        {
            get { return typeof(PipeAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Pipe specific types.

        public void AddAccessRule(PipeAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        public void AddAuditRule(PipeAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        public bool RemoveAccessRule(PipeAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        public void RemoveAccessRuleAll(PipeAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        public void RemoveAccessRuleSpecific(PipeAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        public bool RemoveAuditRule(PipeAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        public void RemoveAuditRuleAll(PipeAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        public void RemoveAuditRuleSpecific(PipeAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        public void ResetAccessRule(PipeAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        public void SetAccessRule(PipeAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        public void SetAuditRule(PipeAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)PipeRights.Read, (int)PipeRights.Write,
                                 (int)PipeRights.Execute, (int)PipeRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

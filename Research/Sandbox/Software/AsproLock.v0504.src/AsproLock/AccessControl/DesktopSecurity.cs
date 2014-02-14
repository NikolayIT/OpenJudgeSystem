using System;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

using Asprosys.Win32;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DesktopSecurity : BaseSecurity
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopSecurity"/> class.
        /// </summary>
        public DesktopSecurity()
            : base(ResourceType.WindowObject, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopSecurity"/> class.
        /// </summary>
        /// <param name="desktopName">Name of the desktop.</param>
        /// <param name="sectionsRequested">The sections requested.</param>
        public DesktopSecurity(string desktopName, AccessControlSections sectionsRequested)
            : base(GetReadHandle(desktopName), ResourceType.WindowObject, sectionsRequested, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopSecurity"/> class.
        /// </summary>
        /// <param name="desktopHandle">The desktop handle.</param>
        /// <param name="sectionsRequested">The sections requested.</param>
        public DesktopSecurity(IntPtr desktopHandle, AccessControlSections sectionsRequested)
            : base(BaseSecurity.GetReadHandle(desktopHandle, NativeMethods.CloseDesktop), ResourceType.WindowObject, sectionsRequested, false)
        {
        }

        private static GenericSafeHandle GetReadHandle(string desktopName)
        {
            IntPtr readHandle = NativeMethods.OpenDesktop(desktopName, 0, false, (int)StandardRights.ReadPermissions);
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
            return new GenericSafeHandle(readHandle, NativeMethods.CloseDesktop);
        }

        #endregion
        
        #region NativeObjectSecurity Abstract Method Overrides

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> of the securable object associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The type of the securable object associated with this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object.
        /// </returns>
        public override Type AccessRightType
        {
            get { return typeof(DesktopRights); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Security.AccessControl.AccessRule"/> class with the specified values.
        /// </summary>
        /// <param name="identityReference">The identity to which the access rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier"/>.</param>
        /// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
        /// <param name="isInherited">true if this rule is inherited from a parent container.</param>
        /// <param name="inheritanceFlags">Specifies the inheritance properties of the access rule.</param>
        /// <param name="propagationFlags">Specifies whether inherited access rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags"/> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None"/>.</param>
        /// <param name="type">Specifies the valid access control type.</param>
        /// <returns>
        /// The <see cref="T:System.Security.AccessControl.AccessRule"/> object that this method creates.
        /// </returns>
        public override AccessRule AccessRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
        {
            return new DesktopAccessRule(identityReference, (DesktopRights)accessMask, type);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> of the object associated with the access rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object. The <see cref="T:System.Type"/> object must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier"/> object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The type of the object associated with the access rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object.
        /// </returns>
        public override Type AccessRuleType
        {
            get { return typeof(DesktopAccessRule); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Security.AccessControl.AuditRule"/> class with the specified values.
        /// </summary>
        /// <param name="identityReference">The identity to which the audit rule applies.  It must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier"/>.</param>
        /// <param name="accessMask">The access mask of this rule. The access mask is a 32-bit collection of anonymous bits, the meaning of which is defined by the individual integrators.</param>
        /// <param name="isInherited">true if this rule is inherited from a parent container.</param>
        /// <param name="inheritanceFlags">Specifies the inheritance properties of the audit rule.</param>
        /// <param name="propagationFlags">Specifies whether inherited audit rules are automatically propagated. The propagation flags are ignored if <paramref name="inheritanceFlags"/> is set to <see cref="F:System.Security.AccessControl.InheritanceFlags.None"/>.</param>
        /// <param name="flags">Specifies the conditions for which the rule is audited.</param>
        /// <returns>
        /// The <see cref="T:System.Security.AccessControl.AuditRule"/> object that this method creates.
        /// </returns>
        public override AuditRule AuditRuleFactory(System.Security.Principal.IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
        {
            return new DesktopAuditRule(identityReference, (DesktopRights)accessMask, flags);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> object associated with the audit rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object. The <see cref="T:System.Type"/> object must be an object that can be cast as a <see cref="T:System.Security.Principal.SecurityIdentifier"/> object.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The type of the object associated with the audit rules of this <see cref="T:System.Security.AccessControl.ObjectSecurity"/> object.
        /// </returns>
        public override Type AuditRuleType
        {
            get { return typeof(DesktopAuditRule); }
        }

        #endregion

        #region AccessControl Overloads for Desktop specific types.

        /// <summary>
        /// Adds the access rule.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        public void AddAccessRule(DesktopAccessRule accessRule)
        {
            base.AddAccessRule(accessRule);
        }

        /// <summary>
        /// Adds the audit rule.
        /// </summary>
        /// <param name="auditRule">The audit rule.</param>
        public void AddAuditRule(DesktopAuditRule auditRule)
        {
            base.AddAuditRule(auditRule);
        }

        /// <summary>
        /// Removes the access rule.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        /// <returns></returns>
        public bool RemoveAccessRule(DesktopAccessRule accessRule)
        {
            return base.RemoveAccessRule(accessRule);
        }

        /// <summary>
        /// Removes the access rule all.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        public void RemoveAccessRuleAll(DesktopAccessRule accessRule)
        {
            base.RemoveAccessRuleAll(accessRule);
        }

        /// <summary>
        /// Removes the access rule specific.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        public void RemoveAccessRuleSpecific(DesktopAccessRule accessRule)
        {
            base.RemoveAccessRuleSpecific(accessRule);
        }

        /// <summary>
        /// Removes the audit rule.
        /// </summary>
        /// <param name="auditRule">The audit rule.</param>
        /// <returns></returns>
        public bool RemoveAuditRule(DesktopAuditRule auditRule)
        {
            return base.RemoveAuditRule(auditRule);
        }

        /// <summary>
        /// Removes the audit rule all.
        /// </summary>
        /// <param name="auditRule">The audit rule.</param>
        public void RemoveAuditRuleAll(DesktopAuditRule auditRule)
        {
            base.RemoveAuditRuleAll(auditRule);
        }

        /// <summary>
        /// Removes the audit rule specific.
        /// </summary>
        /// <param name="auditRule">The audit rule.</param>
        public void RemoveAuditRuleSpecific(DesktopAuditRule auditRule)
        {
            base.RemoveAuditRuleSpecific(auditRule);
        }

        /// <summary>
        /// Resets the access rule.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        public void ResetAccessRule(DesktopAccessRule accessRule)
        {
            base.ResetAccessRule(accessRule);
        }

        /// <summary>
        /// Sets the access rule.
        /// </summary>
        /// <param name="accessRule">The access rule.</param>
        public void SetAccessRule(DesktopAccessRule accessRule)
        {
            base.SetAccessRule(accessRule);
        }

        /// <summary>
        /// Sets the audit rule.
        /// </summary>
        /// <param name="auditRule">The audit rule.</param>
        public void SetAuditRule(DesktopAuditRule auditRule)
        {
            base.SetAuditRule(auditRule);
        }

        #endregion

        #region AccessCheck Methods

        private static GenericMapping m_GenericAccessMapping
            = new GenericMapping((int)DesktopRights.Read, (int)DesktopRights.Write,
                                 (int)DesktopRights.Execute, (int)DesktopRights.AllAccess);

        protected override GenericMapping GetGenericMapping()
        {
            return m_GenericAccessMapping;
        }

        #endregion

    }
}

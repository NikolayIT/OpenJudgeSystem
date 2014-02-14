using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of DesktopRights to be audited
    /// for a user or group. This class cannot be inherited.</summary>
    public sealed class DesktopAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="desktopRights">The desktop rights.</param>
        /// <param name="type">The type.</param>
        public DesktopAuditRule(IdentityReference identity, DesktopRights desktopRights, AuditFlags type)
            : base(identity, (int)desktopRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="desktopRights">The desktop rights.</param>
        /// <param name="type">The type.</param>
        public DesktopAuditRule(string identity, DesktopRights desktopRights, AuditFlags type)
            : this(new NTAccount(identity), desktopRights, type)
        {
        }

        /// <summary>
        /// Gets the desktop rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="DesktopRights"/> values that defines
        /// the rights associated with this rule.</value>
        public DesktopRights DesktopRights
        {
            get
            {
                return (DesktopRights)base.AccessMask;
            }
        }
        
    }
}

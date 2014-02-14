using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>Represents a set of DesktopRights allowed or denied
    /// for a user or group. This class cannot be inherited.</summary>
    public sealed class DesktopAccessRule : AccessRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAccessRule"/> class.
        /// </summary>
        /// <param name="identity">The identity of the user to be granted or denied 
        /// the associated rights.</param>
        /// <param name="desktopRights">The desktop rights allowed or denied.</param>
        /// <param name="type">The type of access controlled by the rule.</param>
        public DesktopAccessRule(IdentityReference identity, DesktopRights desktopRights, AccessControlType type)
            : base(identity, (int)desktopRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopAccessRule"/> class.
        /// </summary>
        /// <param name="identity">The identity of the user to be granted or denied 
        /// the associated rights.</param>
        /// <param name="desktopRights">The desktop rights allowed or denied.</param>
        /// <param name="type">The type of access controlled by the rule.</param>
        public DesktopAccessRule(string identity, DesktopRights desktopRights, AccessControlType type)
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

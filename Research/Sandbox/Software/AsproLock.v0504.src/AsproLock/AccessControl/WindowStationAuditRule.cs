using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WindowStationAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowStationAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="windowStationRights">The window station rights.</param>
        /// <param name="type">The type.</param>
        public WindowStationAuditRule(IdentityReference identity, WindowStationRights windowStationRights, AuditFlags type)
            : base(identity, (int)windowStationRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowStationAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="windowStationRights">The window station rights.</param>
        /// <param name="type">The type.</param>
        public WindowStationAuditRule(string identity, WindowStationRights windowStationRights, AuditFlags type)
            : this(new NTAccount(identity), windowStationRights, type)
        {
        }

        /// <summary>
        /// Gets the window station rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="WindowStationRights"/> values that defines
        /// the rights associated with this rule.</value>
        public WindowStationRights WindowStationRights
        {
            get
            {
                return (WindowStationRights)base.AccessMask;
            }
        }
        
    }
}

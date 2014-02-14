using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    public sealed class WaitObjectAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaitObjectAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="waitObjectRights">The wait object rights.</param>
        /// <param name="type">The type.</param>
        public WaitObjectAuditRule(IdentityReference identity, WaitObjectRights waitObjectRights, AuditFlags type)
            : base(identity, (int)waitObjectRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitObjectAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="waitObjectRights">The wait object rights.</param>
        /// <param name="type">The type.</param>
        public WaitObjectAuditRule(string identity, WaitObjectRights waitObjectRights, AuditFlags type)
            : this(new NTAccount(identity), waitObjectRights, type)
        {
        }

        /// <summary>
        /// Gets the wait object rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="WaitObjectRights"/> values that defines
        /// the rights associated with this rule.</value>
        public WaitObjectRights WaitObjectRights
        {
            get
            {
                return (WaitObjectRights)base.AccessMask;
            }
        }
        
    }
}

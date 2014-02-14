using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProcessAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="processRights">The process rights.</param>
        /// <param name="type">The type.</param>
        public ProcessAuditRule(IdentityReference identity, ProcessRights processRights, AuditFlags type)
            : base(identity, (int)processRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="processRights">The process rights.</param>
        /// <param name="type">The type.</param>
        public ProcessAuditRule(string identity, ProcessRights processRights, AuditFlags type)
            : this(new NTAccount(identity), processRights, type)
        {
        }

        /// <summary>
        /// Gets the process rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="ProcessRights"/> values that defines
        /// the rights associated with this rule.</value>
        public ProcessRights ProcessRights
        {
            get
            {
                return (ProcessRights)base.AccessMask;
            }
        }
        
    }
}

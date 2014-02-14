using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class JobObjectAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobObjectAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="jobObjectRights">The job object rights.</param>
        /// <param name="type">The type.</param>
        public JobObjectAuditRule(IdentityReference identity, JobObjectRights jobObjectRights, AuditFlags type)
            : base(identity, (int)jobObjectRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobObjectAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="jobObjectRights">The job object rights.</param>
        /// <param name="type">The type.</param>
        public JobObjectAuditRule(string identity, JobObjectRights jobObjectRights, AuditFlags type)
            : this(new NTAccount(identity), jobObjectRights, type)
        {
        }

        /// <summary>
        /// Gets the job object rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="JobObjectRights"/> values that defines
        /// the rights associated with this rule.</value>
        public JobObjectRights JobObjectRights
        {
            get
            {
                return (JobObjectRights)base.AccessMask;
            }
        }
        
    }
}

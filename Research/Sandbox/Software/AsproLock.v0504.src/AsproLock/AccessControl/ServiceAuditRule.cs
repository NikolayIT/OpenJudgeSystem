using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="serviceRights">The service rights.</param>
        /// <param name="type">The type.</param>
        public ServiceAuditRule(IdentityReference identity, ServiceRights serviceRights, AuditFlags type)
            : base(identity, (int)serviceRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="serviceRights">The service rights.</param>
        /// <param name="type">The type.</param>
        public ServiceAuditRule(string identity, ServiceRights serviceRights, AuditFlags type)
            : this(new NTAccount(identity), serviceRights, type)
        {
        }

        /// <summary>
        /// Gets the service rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="ServiceRights"/> values that defines
        /// the rights associated with this rule.</value>
        public ServiceRights ServiceRights
        {
            get
            {
                return (ServiceRights)base.AccessMask;
            }
        }
    }
}

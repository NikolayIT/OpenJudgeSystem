using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ThreadAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="threadRights">The thread rights.</param>
        /// <param name="type">The type.</param>
        public ThreadAuditRule(IdentityReference identity, ThreadRights threadRights, AuditFlags type)
            : base(identity, (int)threadRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="threadRights">The thread rights.</param>
        /// <param name="type">The type.</param>
        public ThreadAuditRule(string identity, ThreadRights threadRights, AuditFlags type)
            : this(new NTAccount(identity), threadRights, type)
        {
        }

        /// <summary>
        /// Gets the thread rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="ThreadRights"/> values that defines
        /// the rights associated with this rule.</value>
        public ThreadRights ThreadRights
        {
            get
            {
                return (ThreadRights)base.AccessMask;
            }
        }
        
    }
}

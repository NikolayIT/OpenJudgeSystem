using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PipeAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PipeAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="pipeRights">The pipe rights.</param>
        /// <param name="type">The type.</param>
        public PipeAuditRule(IdentityReference identity, PipeRights pipeRights, AuditFlags type)
            : base(identity, (int)pipeRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="pipeRights">The pipe rights.</param>
        /// <param name="type">The type.</param>
        public PipeAuditRule(string identity, PipeRights pipeRights, AuditFlags type)
            : this(new NTAccount(identity), pipeRights, type)
        {
        }

        /// <summary>
        /// Gets the pipe rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="PipeRights"/> values that defines
        /// the rights associated with this rule.</value>
        public PipeRights PipeRights
        {
            get
            {
                return (PipeRights)base.AccessMask;
            }
        }
        
    }
}

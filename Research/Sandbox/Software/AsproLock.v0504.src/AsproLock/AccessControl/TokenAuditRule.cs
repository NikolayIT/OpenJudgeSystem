using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TokenAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="tokenRights">The token rights.</param>
        /// <param name="type">The type.</param>
        public TokenAuditRule(IdentityReference identity, TokenRights tokenRights, AuditFlags type)
            : base(identity, (int)tokenRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="tokenRights">The token rights.</param>
        /// <param name="type">The type.</param>
        public TokenAuditRule(string identity, TokenRights tokenRights, AuditFlags type)
            : this(new NTAccount(identity), tokenRights, type)
        {
        }

        /// <summary>
        /// Gets the token rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="TokenRights"/> values that defines
        /// the rights associated with this rule.</value>
        public TokenRights TokenRights
        {
            get
            {
                return (TokenRights)base.AccessMask;
            }
        }
        
    }
}

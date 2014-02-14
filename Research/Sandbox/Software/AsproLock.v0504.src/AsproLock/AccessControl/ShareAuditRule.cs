using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public class ShareAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShareAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="shareRights">The share rights.</param>
        /// <param name="type">The type.</param>
        public ShareAuditRule(IdentityReference identity, ShareRights shareRights, AuditFlags type)
            : base(identity, (int)shareRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="shareRights">The share rights.</param>
        /// <param name="type">The type.</param>
        public ShareAuditRule(string identity, ShareRights shareRights, AuditFlags type)
            : this(new NTAccount(identity), shareRights, type)
        {
        }

        /// <summary>
        /// Gets the share rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="ShareRights"/> values that defines
        /// the rights associated with this rule.</value>
        public ShareRights ShareRights
        {
            get
            {
                return (ShareRights)base.AccessMask;
            }
        }
    }
}

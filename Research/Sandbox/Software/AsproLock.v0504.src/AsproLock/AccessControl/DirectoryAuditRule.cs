using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DirectoryAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="directoryRights">The directory rights.</param>
        /// <param name="type">The type.</param>
        public DirectoryAuditRule(IdentityReference identity, DirectoryRights directoryRights, AuditFlags type)
            : base(identity, (int)directoryRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="directoryRights">The directory rights.</param>
        /// <param name="type">The type.</param>
        public DirectoryAuditRule(string identity, DirectoryRights directoryRights, AuditFlags type)
            : this(new NTAccount(identity), directoryRights, type)
        {
        }

        /// <summary>
        /// Gets the directory rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="DirectoryRights"/> values that defines
        /// the rights associated with this rule.</value>
        public DirectoryRights DirectoryRights
        {
            get
            {
                return (DirectoryRights)base.AccessMask;
            }
        }
        
    }
}

using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="fileRights">The file rights.</param>
        /// <param name="type">The type.</param>
        public FileAuditRule(IdentityReference identity, FileRights fileRights, AuditFlags type)
            : base(identity, (int)fileRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="fileRights">The file rights.</param>
        /// <param name="type">The type.</param>
        public FileAuditRule(string identity, FileRights fileRights, AuditFlags type)
            : this(new NTAccount(identity), fileRights, type)
        {
        }

        /// <summary>
        /// Gets the file rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="FileRights"/> values that defines
        /// the rights associated with this rule.</value>
        public FileRights FileRights
        {
            get
            {
                return (FileRights)base.AccessMask;
            }
        }
        
    }
}

using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileMappingAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileMappingAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="fileMappingRights">The file mapping rights.</param>
        /// <param name="type">The type.</param>
        public FileMappingAuditRule(IdentityReference identity, FileMappingRights fileMappingRights, AuditFlags type)
            : base(identity, (int)fileMappingRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMappingAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="fileMappingRights">The file mapping rights.</param>
        /// <param name="type">The type.</param>
        public FileMappingAuditRule(string identity, FileMappingRights fileMappingRights, AuditFlags type)
            : this(new NTAccount(identity), fileMappingRights, type)
        {
        }

        /// <summary>
        /// Gets the file mapping rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="FileMappingRights"/> values that defines
        /// the rights associated with this rule.</value>
        public FileMappingRights FileMappingRights
        {
            get
            {
                return (FileMappingRights)base.AccessMask;
            }
        }
        
    }
}

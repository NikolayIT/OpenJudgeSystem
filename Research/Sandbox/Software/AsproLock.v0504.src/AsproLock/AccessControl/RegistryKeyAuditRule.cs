using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Asprosys.Security.AccessControl
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RegistryKeyAuditRule : AuditRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryKeyAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="registryKeyRights">The registry key rights.</param>
        /// <param name="type">The type.</param>
        public RegistryKeyAuditRule(IdentityReference identity, RegistryKeyRights registryKeyRights, AuditFlags type)
            : base(identity, (int)registryKeyRights, false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryKeyAuditRule"/> class.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="registryKeyRights">The registry key rights.</param>
        /// <param name="type">The type.</param>
        public RegistryKeyAuditRule(string identity, RegistryKeyRights registryKeyRights, AuditFlags type)
            : this(new NTAccount(identity), registryKeyRights, type)
        {
        }

        /// <summary>
        /// Gets the registry key rights associated with this rule.
        /// </summary>
        /// <value>The combination of <see cref="RegistryKeyRights"/> values that defines
        /// the rights associated with this rule.</value>
        public RegistryKeyRights RegistryKeyRights
        {
            get
            {
                return (RegistryKeyRights)base.AccessMask;
            }
        }
        
    }
}
